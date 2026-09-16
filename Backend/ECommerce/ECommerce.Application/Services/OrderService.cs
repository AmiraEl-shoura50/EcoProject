using AutoMapper;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Services;

public class OrderService : IOrderService
{
    
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
     
    private readonly IFileStorageService _fileStorageService;
    private readonly IPaymentService _paymentService; // ✅ جديد - ضيفيها في الـ Constructor
    public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService, INotificationService notificationService, IFileStorageService fileStorageService, IPaymentService paymentService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _emailService = emailService;
        _notificationService = notificationService;
        _fileStorageService = fileStorageService;
        _paymentService = paymentService;
    }

   

    public async Task<IEnumerable<CheckoutResultDto>> CheckoutAsync(int customerId, CreateOrderDto dto)
    {
        var cart = await _unitOfWork.Carts.GetByCustomerIdAsync(customerId);
        if (cart is null || !cart.Items.Any())
            return Enumerable.Empty<CheckoutResultDto>();

        var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(dto.PaymentMethodId);
        if (paymentMethod is null)
            return Enumerable.Empty<CheckoutResultDto>();

        foreach (var item in cart.Items)
        {
            if (item.Product.StockQuantity < item.Quantity)
                return Enumerable.Empty<CheckoutResultDto>();
        }

        var groupedBySeller = cart.Items.GroupBy(i => i.Product.SellerId);
        var createdOrderIds = new List<(int OrderId, int SellerId)>();

        foreach (var group in groupedBySeller)
        {
            var order = new Order
            {
                CustomerId = customerId,
                PaymentMethodId = dto.PaymentMethodId,
                Status = OrderStatus.Pending,
                CreatedDate = DateTime.UtcNow,
                Items = new List<OrderItem>()
            };

            decimal totalAmount = 0;

            foreach (var item in group)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                });

                totalAmount += item.Product.Price * item.Quantity;
                item.Product.StockQuantity -= item.Quantity;
                _unitOfWork.Products.Update(item.Product);
            }

            order.TotalAmount = totalAmount;
            await _unitOfWork.Orders.AddAsync(order);
            createdOrderIds.Add((order.Id, group.Key)); // ✅ Id بيتحدد بعد SaveChanges - هنعدلها تحت
        }

        cart.Items.Clear();
        await _unitOfWork.SaveChangesAsync(); // ✅ دلوقتي كل الـ order.Id بقت متاحة

        var results = new List<CheckoutResultDto>();

        //foreach (var group in groupedBySeller)
        //{
        //    var order = group.First().Cart.Items == null ? null : null; // (تجاهلي السطر ده - هيتشال، موجود بس عشان التوضيح)
        //}

        // ✅ الطريقة الأنضف - نلف تاني على الأوردرات اللي اتعملت فعليًا من الداتابيز
        var allCreatedOrders = new List<Order>();
        foreach (var (orderId, sellerId) in createdOrderIds)
        {
            var seller = await _unitOfWork.Sellers.GetByIdAsync(sellerId);
            if (seller is not null)
            {
                await _notificationService.CreateAndSendAsync(
                    seller.UserId, "طلب جديد! 📦", $"وصلك طلب جديد رقم #{orderId}");
            }

            var full = await _unitOfWork.Orders.GetWithItemsAsync(orderId);
            if (full is null) continue;
            allCreatedOrders.Add(full);
        }

        foreach (var order in allCreatedOrders)
        {
            string? paymentUrl = null;

            if (paymentMethod.Type == PaymentMethodType.Gateway)
            {
                paymentUrl = await _paymentService.InitiateGatewayPaymentAsync(order.Id);
            }
            else
            {
                var customerEmail = order.Customer?.User?.Email;
                if (!string.IsNullOrEmpty(customerEmail))
                {
                    _ = _emailService.SendEmailAsync(customerEmail,
                        $"طلبك #{order.Id} في انتظار تأكيد الدفع",
                        $"<p>يرجى تحويل {order.TotalAmount} جنيه عبر {order.PaymentMethod.MethodName} ثم رفع إثبات التحويل</p>");
                }
            }

            var refreshedOrder = await _unitOfWork.Orders.GetWithItemsAsync(order.Id); // ✅ لأخذ الـ Status المحدّث بعد InitiateGatewayPaymentAsync
            results.Add(new CheckoutResultDto
            {
                Order = _mapper.Map<OrderDto>(refreshedOrder),
                PaymentUrl = paymentUrl
            });
        }

        return results;
    }

    public async Task<OrderDto?> GetByIdAsync(int orderId, int customerId)
    {
        var order = await _unitOfWork.Orders.GetWithItemsAsync(orderId);
        if (order is null || order.CustomerId != customerId) return null;

        return _mapper.Map<OrderDto>(order);
    }

   
    public async Task<PaymentProofDto?> SubmitPaymentProofAsync(int customerId, int orderId, SubmitPaymentProofDto dto)
    {
        var order = await _unitOfWork.Orders.GetWithItemsAsync(orderId);
        if (order is null || order.CustomerId != customerId) return null;

        // ✅ الميزة دي بس لطريقة الدفع اليدوية
        if (order.PaymentMethod.Type != PaymentMethodType.Manual) return null;

        if (order.Status != OrderStatus.Pending) return null;

        using var stream = dto.Image.OpenReadStream();
        var imageUrl = await _fileStorageService.UploadImageAsync(stream, dto.Image.FileName, "payment-proofs");

        var proof = new PaymentProof
        {
            OrderId = orderId,
            ImageUrl = imageUrl,
            TransferReference = dto.TransferReference,
            SubmittedAt = DateTime.UtcNow,
            IsApproved = null
        };

        await _unitOfWork.PaymentProofs.AddAsync(proof);

        order.Status = OrderStatus.AwaitingConfirmation;
        _unitOfWork.Orders.Update(order);

        await _unitOfWork.SaveChangesAsync();

        var sellerId = order.Items.First().Product.SellerId;
        var seller = await _unitOfWork.Sellers.GetByIdAsync(sellerId);
        if (seller is not null)
        {
            await _notificationService.CreateAndSendAsync(
                seller.UserId, "إثبات دفع جديد 🧾", $"العميل أرسل إثبات دفع للطلب #{orderId}، برجاء المراجعة");
        }

        return _mapper.Map<PaymentProofDto>(proof);
    }

    public async Task<bool> ReviewPaymentProofAsync(int sellerId, int orderId, ReviewPaymentProofDto dto)
    {
        var order = await _unitOfWork.Orders.GetWithItemsAsync(orderId);
        if (order is null) return false;

        var hasSellerProduct = order.Items.Any(i => i.Product.SellerId == sellerId);
        if (!hasSellerProduct) return false;

        if (order.Status != OrderStatus.AwaitingConfirmation) return false;

        var latestProof = order.PaymentProofs
            .Where(p => p.IsApproved == null)
            .OrderByDescending(p => p.SubmittedAt)
            .FirstOrDefault();

        if (latestProof is null) return false;

        latestProof.ReviewedAt = DateTime.UtcNow;
        latestProof.IsApproved = dto.Approve;

        if (dto.Approve)
        {
            order.Status = OrderStatus.Confirmed;
        }
        else
        {
            latestProof.RejectionReason = dto.RejectionReason;
            order.Status = OrderStatus.Pending;
        }

        _unitOfWork.PaymentProofs.Update(latestProof);
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        await _notificationService.CreateAndSendAsync(
            order.Customer.UserId,
            dto.Approve ? "تم تأكيد الدفع ✅" : "تم رفض إثبات الدفع ❌",
            dto.Approve
                ? $"تم تأكيد استلام دفعتك للطلب #{order.Id}"
                : $"تم رفض إثبات الدفع للطلب #{order.Id}. السبب: {dto.RejectionReason ?? "غير محدد"}");

        return true;
    }

    public async Task<PaginatedResultDto<OrderDto>> GetMyOrdersAsync (int customerId, int pageNumber, int pageSize)
    {
        var (items, totalCount) = await _unitOfWork.Orders.GetByCustomerPagedAsync(customerId, pageNumber, pageSize);

        return new PaginatedResultDto<OrderDto>
        {
            Items = _mapper.Map<List<OrderDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

 

    public async Task<bool> CancelAsync(int orderId, int customerId)
    {
        var order = await _unitOfWork.Orders.GetWithItemsAsync(orderId);
        if (order is null || order.CustomerId != customerId) return false;

        if (order.Status != OrderStatus.Pending &&
            order.Status != OrderStatus.AwaitingConfirmation &&
            order.Status != OrderStatus.Confirmed)
            return false;

        foreach (var item in order.Items)
        {
            item.Product.StockQuantity += item.Quantity;
            _unitOfWork.Products.Update(item.Product);
        }

        order.Status = OrderStatus.Cancelled;
        _unitOfWork.Orders.Update(order);

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersForSellerAsync(int sellerId)
    {
        var orders = await _unitOfWork.Orders.GetBySellerAsync(sellerId);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<bool> UpdateStatusAsync(int orderId, int sellerId, string newStatus)
    {
        var order = await _unitOfWork.Orders.GetWithItemsAsync(orderId);
        if (order is null) return false;

        var hasSellerProduct = order.Items.Any(i => i.Product.SellerId == sellerId);
        if (!hasSellerProduct) return false;

        if (!Enum.TryParse<OrderStatus>(newStatus, true, out var status)) return false;

        var validTransitions = new Dictionary<OrderStatus, OrderStatus[]>
        {
            [OrderStatus.Confirmed] = new[] { OrderStatus.Shipped, OrderStatus.Cancelled },
            [OrderStatus.Shipped] = new[] { OrderStatus.Delivered },
        };

        if (!validTransitions.TryGetValue(order.Status, out var allowedNext) || !allowedNext.Contains(status))
            return false;

        order.Status = status;
        if (status == OrderStatus.Delivered)
        {
            order.CompletedDate = DateTime.UtcNow;
        }

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        await _notificationService.CreateAndSendAsync(
            order.Customer.UserId, "تحديث حالة الطلب", $"طلبك #{order.Id} أصبح: {status}");

        return true;
    }
}