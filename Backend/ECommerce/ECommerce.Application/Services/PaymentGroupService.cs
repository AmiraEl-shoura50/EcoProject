using AutoMapper;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.DTOs.Payment;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services;

public class PaymentGroupService : IPaymentGroupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;
    private readonly INotificationService _notificationService;
    private readonly IAdminService _adminService;
    private readonly ILogger<PaymentGroupService> _logger;

    public PaymentGroupService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IFileStorageService fileStorageService,
        INotificationService notificationService,
        IAdminService adminService,
        ILogger<PaymentGroupService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
        _notificationService = notificationService;
        _adminService = adminService;
        _logger = logger;
    }

    // ================= العميل =================

    public async Task<PaymentGroupDto?> GetForCustomerAsync(int groupId, int customerId)
    {
        var group = await _unitOfWork.PaymentGroups.GetWithDetailsAsync(groupId);
        if (group is null || group.CustomerId != customerId) return null;

        var dto = new PaymentGroupDto();
        Fill(dto, group);
        return dto;
    }

    public async Task<List<PendingPaymentDto>> GetPendingForCustomerAsync(int customerId)
    {
        var groups = await _unitOfWork.PaymentGroups.GetPendingForCustomerAsync(customerId);

        return groups.Select(group =>
        {
            var active = ActiveOrders(group);

            return new PendingPaymentDto
            {
                Id = group.Id,
                TotalAmount = active.Sum(o => o.TotalAmount),
                CreatedAt = group.CreatedAt,
                PaymentMethodName = group.PaymentMethod.MethodName,
                OrdersCount = active.Count
            };
        }).ToList();
    }

    public async Task<PaymentProofDto?> SubmitProofAsync(int groupId, int customerId, SubmitPaymentProofDto dto)
    {
        var group = await _unitOfWork.PaymentGroups.GetWithDetailsAsync(groupId);
        if (group is null || group.CustomerId != customerId) return null;
        if (group.Status != PaymentGroupStatus.Pending || group.PaymentMethod.Type != PaymentMethodType.Manual) return null;

        var active = ActiveOrders(group);
        if (active.Count == 0) return null;

        using var stream = dto.Image.OpenReadStream();
        var imageUrl = await _fileStorageService.UploadImageAsync(stream, dto.Image.FileName, "payment-proofs");

        var proof = new PaymentProof
        {
            PaymentGroupId = group.Id,
            ImageUrl = imageUrl,
            TransferReference = string.IsNullOrWhiteSpace(dto.TransferReference) ? null : dto.TransferReference.Trim(),
            SubmittedAt = DateTime.UtcNow,
            IsApproved = null
        };

        await _unitOfWork.PaymentProofs.AddAsync(proof);

        group.Status = PaymentGroupStatus.AwaitingConfirmation;

        foreach (var order in active.Where(o => o.Status == OrderStatus.Pending))
            order.Status = OrderStatus.AwaitingConfirmation;

        await _unitOfWork.SaveChangesAsync();

        await NotifyAdminsAsync(group, active.Sum(o => o.TotalAmount));

        return _mapper.Map<PaymentProofDto>(proof);
    }

    // ================= الأدمن =================

    public async Task<PaginatedResultDto<AdminPaymentGroupDto>> GetForAdminAsync(
        string? filter, string? search, int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (groups, totalCount) = await _unitOfWork.PaymentGroups.GetForAdminAsync(filter, search, pageNumber, pageSize);

        var items = groups.Select(group =>
        {
            var user = group.Customer.User;

            var dto = new AdminPaymentGroupDto
            {
                CustomerName = $"{user.FirstName} {user.LastName}".Trim(),
                CustomerEmail = user.Email ?? string.Empty,
                CustomerPhone = user.PhoneNumber,
                Proofs = _mapper.Map<List<PaymentProofDto>>(group.Proofs.OrderByDescending(p => p.SubmittedAt))
            };

            Fill(dto, group);
            return dto;
        }).ToList();

        return new PaginatedResultDto<AdminPaymentGroupDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public Task<int> GetAwaitingCountAsync()
        => _unitOfWork.PaymentGroups.CountAwaitingAsync();

    public async Task<PaymentReviewResult> ReviewAsync(int groupId, ReviewPaymentProofDto dto)
    {
        var group = await _unitOfWork.PaymentGroups.GetWithDetailsAsync(groupId);
        if (group is null) return PaymentReviewResult.NotFound;
        if (group.Status != PaymentGroupStatus.AwaitingConfirmation) return PaymentReviewResult.InvalidState;

        var proof = group.Proofs
            .Where(p => p.IsApproved == null)
            .OrderByDescending(p => p.SubmittedAt)
            .FirstOrDefault();

        var active = ActiveOrders(group);
        if (proof is null || active.Count == 0) return PaymentReviewResult.InvalidState;

        proof.ReviewedAt = DateTime.UtcNow;
        proof.IsApproved = dto.Approve;

        if (dto.Approve)
        {
            group.Status = PaymentGroupStatus.Paid;
            group.PaidAt = DateTime.UtcNow;

            foreach (var order in active.Where(o => o.Status == OrderStatus.AwaitingConfirmation))
                order.Status = OrderStatus.Confirmed;
        }
        else
        {
            proof.RejectionReason = dto.RejectionReason?.Trim();
            group.Status = PaymentGroupStatus.Pending;

            foreach (var order in active.Where(o => o.Status == OrderStatus.AwaitingConfirmation))
                order.Status = OrderStatus.Pending;
        }

        await _unitOfWork.SaveChangesAsync();

        var total = active.Sum(o => o.TotalAmount);
        var customerUserId = group.Customer.UserId;

        if (dto.Approve)
        {
            await TryNotifyAsync(customerUserId, "تم تأكيد الدفع ✅",
                $"تم تأكيد دفعتك بقيمة {total:0.##} جنيه، وبدأ البائعون تجهيز طلباتك", "/orders");

            // كل بائع بيوصله إشعار بطلبه هو بس
            foreach (var order in active)
            {
                var seller = order.Items.First().Product.Seller;

                await TryNotifyAsync(seller.UserId, "طلب جديد مدفوع 📦",
                    $"تم تأكيد دفع الطلب #{order.Id}، جهّز الطلب للشحن", "/seller/orders");
            }
        }
        else
        {
            await TryNotifyAsync(customerUserId, "تم رفض إثبات الدفع ❌",
                $"تم رفض إثبات الدفع للدفعة #{group.Id}. السبب: {proof.RejectionReason ?? "غير محدد"}. ارفع إثباتًا جديدًا",
                $"/payments/{group.Id}");
        }

        return PaymentReviewResult.Ok;
    }

    // ================= مساعدات =================

    private static List<Order> ActiveOrders(PaymentGroup group)
        => group.Orders.Where(o => o.Status != OrderStatus.Cancelled).ToList();

    private void Fill(PaymentGroupDto dto, PaymentGroup group)
    {
        var active = ActiveOrders(group);

        dto.Id = group.Id;
        dto.Status = active.Count == 0 ? "Cancelled" : group.Status.ToString();
        dto.TotalAmount = active.Sum(o => o.TotalAmount);
        dto.CreatedAt = group.CreatedAt;
        dto.PaymentMethodName = group.PaymentMethod.MethodName;
        dto.TransferUrl = group.PaymentMethod.TransferUrl;
        dto.CanUploadProof = group.Status == PaymentGroupStatus.Pending
                             && active.Count > 0
                             && group.PaymentMethod.Type == PaymentMethodType.Manual;

        var lastProof = group.Proofs.OrderByDescending(p => p.SubmittedAt).FirstOrDefault();
        dto.LastRejectionReason = group.Status == PaymentGroupStatus.Pending && lastProof is { IsApproved: false }
            ? lastProof.RejectionReason
            : null;

        dto.Orders = group.Orders
            .OrderBy(o => o.Id)
            .Select(order =>
            {
                var seller = order.Items.First().Product.Seller;

                return new PaymentGroupOrderDto
                {
                    OrderId = order.Id,
                    SellerId = seller.Id,
                    StoreName = seller.StoreName,
                    Amount = order.TotalAmount,
                    Status = order.Status.ToString(),
                    Items = _mapper.Map<List<OrderItemDto>>(order.Items)
                };
            })
            .ToList();
    }

    private async Task NotifyAdminsAsync(PaymentGroup group, decimal total)
    {
        try
        {
            var user = group.Customer.User;
            var customerName = $"{user.FirstName} {user.LastName}".Trim();

            foreach (var adminId in await _adminService.GetAdminIdsAsync())
            {
                await _notificationService.CreateAndSendAsync(adminId, "إثبات دفع جديد 🧾",
                    $"الدفعة #{group.Id} بقيمة {total:0.##} جنيه من {customerName} في انتظار المراجعة",
                    "/admin/payments");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "تعذر إرسال إشعار الإثبات الجديد للأدمن");
        }
    }

    // بأقصى جهد: التغيير اتحفظ، وفشل الإشعار ميكسرش العملية
    private async Task TryNotifyAsync(Guid userId, string title, string message, string link)
    {
        try
        {
            await _notificationService.CreateAndSendAsync(userId, title, message, link);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "تعذر إرسال الإشعار للمستخدم {UserId}", userId);
        }
    }
}