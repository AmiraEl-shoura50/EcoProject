using System.Net;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Services;

public class CheckoutService : ICheckoutService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPricingService _pricingService;
    private readonly IEmailService _emailService;

    public CheckoutService(IUnitOfWork unitOfWork, IPricingService pricingService, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _pricingService = pricingService;
        _emailService = emailService;
    }

    public async Task<CheckoutResponseDto?> CheckoutAsync(int customerId, int paymentMethodId)
    {
        var cart = await _unitOfWork.Carts.GetByCustomerIdAsync(customerId);
        if (cart is null || !cart.Items.Any()) return null;

        var method = await _unitOfWork.PaymentMethods.GetByIdAsync(paymentMethodId);
        if (method is null || method.IsHidden || method.Type != PaymentMethodType.Manual) return null;

        var unavailable = await _unitOfWork.Products.GetUnavailableIdsAsync(cart.Items.Select(i => i.ProductId).ToList());
        if (unavailable.Count > 0) return null;

        if (cart.Items.Any(i => i.Product.StockQuantity < i.Quantity)) return null;

        // دفتر الأسعار بيتحمّل مرة واحدة لكل الدفعة
        var priceBook = await _pricingService.LoadAsync();

        var group = new PaymentGroup
        {
            CustomerId = customerId,
            PaymentMethodId = paymentMethodId,
            Status = PaymentGroupStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // طلب لكل بائع، كلهم تابعين لدفعة واحدة
        foreach (var sellerItems in cart.Items.GroupBy(i => i.Product.SellerId))
        {
            var order = new Order
            {
                CustomerId = customerId,
                PaymentMethodId = paymentMethodId,
                Status = OrderStatus.Pending,
                CreatedDate = DateTime.UtcNow,
                Items = new List<OrderItem>()
            };

            decimal total = 0;

            foreach (var item in sellerItems)
            {
                // السعر بعد الخصم لحظة الشراء، وده اللي بيتسجل ويتحاسب بيه العميل
                var unitPrice = priceBook.GetPrice(item.Product).Final;

                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice
                });

                total += unitPrice * item.Quantity;
                item.Product.StockQuantity -= item.Quantity;
                _unitOfWork.Products.Update(item.Product);
            }

            order.TotalAmount = total;
            group.Orders.Add(order);
        }

        await _unitOfWork.PaymentGroups.AddAsync(group);
        cart.Items.Clear();

        // ✅ حفظ واحد: الدفعة والطلبات والمخزون وتفريغ السلة بيتحفظوا مع بعض أو مفيش حاجة تتحفظ
        await _unitOfWork.SaveChangesAsync();

        var totalAmount = group.Orders.Sum(o => o.TotalAmount);
        await SendPaymentEmailAsync(customerId, method, totalAmount);

        return new CheckoutResponseDto { PaymentGroupId = group.Id, TotalAmount = totalAmount };
    }

    private async Task SendPaymentEmailAsync(int customerId, PaymentMethod method, decimal totalAmount)
    {
        var customer = await _unitOfWork.Customers.GetWithUserAsync(customerId);
        var email = customer?.User.Email;
        if (string.IsNullOrEmpty(email)) return;

        var transferLink = string.IsNullOrEmpty(method.TransferUrl)
            ? string.Empty
            : $"<p><a href=\"{WebUtility.HtmlEncode(method.TransferUrl)}\">اضغط هنا لإتمام التحويل</a></p>";

        _ = _emailService.SendEmailAsync(
            email,
            "طلبك في انتظار الدفع",
            $"<p>يرجى تحويل <b>{totalAmount:0.##} جنيه</b> (إجمالي طلبك) عبر {WebUtility.HtmlEncode(method.MethodName)} ثم رفع إثبات التحويل من صفحة الدفع في الموقع.</p>{transferLink}");
    }
}