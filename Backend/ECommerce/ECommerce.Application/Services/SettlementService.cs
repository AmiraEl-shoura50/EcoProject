using System.Globalization;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Settlement;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services;

public class SettlementService : ISettlementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExcelReportService _excel;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SettlementService> _logger;

    public SettlementService(
        IUnitOfWork unitOfWork,
        IExcelReportService excel,
        INotificationService notificationService,
        ILogger<SettlementService> logger)
    {
        _unitOfWork = unitOfWork;
        _excel = excel;
        _notificationService = notificationService;
        _logger = logger;
    }

    // ================= الأدمن =================

    public async Task<List<SellerBalanceDto>> GetBalancesAsync(string cutoff, int tzOffset)
    {
        var cutoffUtc = CutoffUtc(cutoff, tzOffset, out _);

        var rows = await _unitOfWork.Settlements.GetUnsettledOrderRowsAsync(cutoffUtc, null);
        var sellers = await _unitOfWork.Sellers.GetAllWithUserAsync();
        var paid = await _unitOfWork.Settlements.GetPaidTotalsAsync(null);
        var rowsBySeller = rows.ToLookup(r => r.SellerId);

        return sellers
            .Select(seller =>
            {
                var sellerRows = rowsBySeller[seller.Id].ToList();
                var (gross, commission) = Calculate(sellerRows, seller.CommissionPercent);
                paid.TryGetValue(seller.Id, out var paidTotal);

                return new SellerBalanceDto
                {
                    SellerId = seller.Id,
                    StoreName = seller.StoreName,
                    ImageUrl = seller.ImageUrl,
                    OwnerName = $"{seller.User.FirstName} {seller.User.LastName}".Trim(),
                    OwnerEmail = seller.User.Email ?? string.Empty,
                    OwnerPhone = seller.User.PhoneNumber,
                    CommissionPercent = seller.CommissionPercent,
                    PayoutMethod = seller.PayoutMethod?.ToString(),
                    PayoutAccount = seller.PayoutAccount,
                    OrdersCount = sellerRows.Count,
                    GrossAmount = gross,
                    CommissionAmount = commission,
                    NetAmount = gross - commission,
                    TotalPaid = paidTotal?.Total ?? 0,
                    SettlementsCount = paidTotal?.Count ?? 0,
                    BlockReason = sellerRows.Count == 0 ? "لا توجد طلبات متوصّلة مستحقة" : BlockReason(seller)
                };
            })
            .OrderByDescending(b => b.OrdersCount > 0)
            .ThenByDescending(b => b.NetAmount)
            .ThenBy(b => b.StoreName)
            .ToList();
    }

    public async Task<bool> SetCommissionAsync(int sellerId, decimal percent)
    {
        var seller = await _unitOfWork.Sellers.GetByIdAsync(sellerId);
        if (seller is null) return false;

        seller.CommissionPercent = Math.Round(percent, 2);

        _unitOfWork.Sellers.Update(seller);
        await _unitOfWork.SaveChangesAsync();

        await TryNotifyAsync(seller.UserId, "تم تحديد نسبة عمولة الموقع",
            $"نسبة عمولة الموقع على مبيعاتك أصبحت {seller.CommissionPercent:0.##}%", "/seller/earnings");

        return true;
    }

    public async Task<SettlementDto> CreateAsync(int sellerId, CreateSettlementDto dto, Guid adminUserId)
    {
        var seller = await _unitOfWork.Sellers.GetWithUserAsync(sellerId)
            ?? throw new KeyNotFoundException("البائع غير موجود");

        var cutoffUtc = CutoffUtc(dto.Cutoff, dto.TzOffset, out var cutoffDay);
        var rows = await _unitOfWork.Settlements.GetUnsettledOrderRowsAsync(cutoffUtc, sellerId);

        if (rows.Count == 0)
            throw new InvalidOperationException("لا توجد طلبات متوصّلة مستحقة لهذا البائع حتى التاريخ المحدد");

        // لو اتوصّلت طلبات جديدة بعد ما الأدمن شاف المعاينة، الأرقام اتغيّرت ولازم يراجعها
        if (rows.Count != dto.ExpectedOrdersCount)
            throw new InvalidOperationException("الأرقام اتغيّرت (طلبات جديدة اتوصّلت)، حدّث الصفحة وراجع المبلغ قبل التحويل");

        var reason = BlockReason(seller);
        if (reason is not null) throw new InvalidOperationException(reason);

        var percent = seller.CommissionPercent!.Value;
        var (gross, commission) = Calculate(rows, percent);
        var adjustment = Math.Round(dto.AdjustmentAmount, 2);
        var net = gross - commission + adjustment;

        if (net <= 0)
            throw new InvalidOperationException("صافي المبلغ لازم يكون أكبر من صفر");

        var settlement = new Settlement
        {
            SellerId = sellerId,
            CreatedAt = DateTime.UtcNow,
            CutoffDay = cutoffDay,
            OrdersCount = rows.Count,
            GrossAmount = gross,
            CommissionPercent = percent,
            CommissionAmount = commission,
            AdjustmentAmount = adjustment,
            AdjustmentNote = adjustment == 0 ? null : dto.AdjustmentNote?.Trim(),
            NetAmount = net,
            PayoutMethod = seller.PayoutMethod!.Value.ToString(),
            PayoutAccount = seller.PayoutAccount!,
            Reference = string.IsNullOrWhiteSpace(dto.Reference) ? null : dto.Reference.Trim(),
            CreatedByUserId = adminUserId
        };

        // إنشاء التسوية وقفل الطلبات في معاملة واحدة
        var created = await _unitOfWork.Settlements.TryCreateAsync(settlement, rows.Select(r => r.OrderId).ToList());
        if (!created)
            throw new InvalidOperationException("بعض الطلبات اتسوّت بالفعل، حدّث الصفحة وحاول مرة أخرى");

        await TryNotifyAsync(seller.UserId, "تم تحويل مستحقاتك 💰",
            $"تم تحويل {net:0.##} جنيه عن {rows.Count} طلب" +
            (settlement.Reference is null ? string.Empty : $" (رقم العملية: {settlement.Reference})"),
            "/seller/earnings");

        return ToDto(settlement, seller.StoreName);
    }

    public async Task<PaginatedResultDto<SettlementDto>> GetHistoryAsync(int? sellerId, int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, totalCount) = await _unitOfWork.Settlements.GetPagedAsync(sellerId, pageNumber, pageSize);

        return new PaginatedResultDto<SettlementDto>
        {
            Items = items.Select(s => ToDto(s, s.Seller.StoreName)).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<byte[]?> ExportSettlementAsync(int settlementId, int? sellerId, int tzOffset)
    {
        var settlement = await _unitOfWork.Settlements.GetWithOrdersAsync(settlementId);
        if (settlement is null) return null;
        if (sellerId.HasValue && settlement.SellerId != sellerId.Value) return null; // البائع بيحمّل تقاريره هو بس

        DateTime Local(DateTime utc) => utc.AddMinutes(-tzOffset);

        var user = settlement.Seller.User;

        var data = new SettlementReportData
        {
            SettlementId = settlement.Id,
            StoreName = settlement.Seller.StoreName,
            OwnerName = $"{user.FirstName} {user.LastName}".Trim(),
            OwnerEmail = user.Email ?? string.Empty,
            OwnerPhone = user.PhoneNumber,
            PayoutMethod = settlement.PayoutMethod,
            PayoutAccount = settlement.PayoutAccount,
            Reference = settlement.Reference,
            CreatedLocal = Local(settlement.CreatedAt),
            CutoffDay = settlement.CutoffDay,
            OrdersCount = settlement.OrdersCount,
            GrossAmount = settlement.GrossAmount,
            CommissionPercent = settlement.CommissionPercent,
            CommissionAmount = settlement.CommissionAmount,
            AdjustmentAmount = settlement.AdjustmentAmount,
            AdjustmentNote = settlement.AdjustmentNote,
            NetAmount = settlement.NetAmount
        };

        foreach (var order in settlement.Orders.OrderBy(o => o.CompletedDate ?? o.CreatedDate))
        {
            var commission = CommissionOf(order.TotalAmount, settlement.CommissionPercent);

            data.Orders.Add(new ReportOrderRow
            {
                OrderId = order.Id,
                CreatedLocal = Local(order.CreatedDate),
                DeliveredLocal = Local(order.CompletedDate ?? order.CreatedDate),
                PaymentMethod = order.PaymentMethod.MethodName,
                ItemsCount = order.Items.Sum(i => i.Quantity),
                Amount = order.TotalAmount,
                Commission = commission,
                Net = order.TotalAmount - commission
            });

            foreach (var item in order.Items)
            {
                data.Items.Add(new ReportItemRow
                {
                    OrderId = order.Id,
                    ProductName = item.Product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.UnitPrice * item.Quantity
                });
            }
        }

        return _excel.BuildSettlementReport(data);
    }

    public async Task<byte[]> ExportPeriodReportAsync(string from, string to, int tzOffset)
    {
        var fromUtc = CutoffUtc(from, tzOffset, out var fromDay, endOfDay: false);
        var toUtc = CutoffUtc(to, tzOffset, out var toDay);

        if (toUtc <= fromUtc)
            throw new InvalidOperationException("تاريخ النهاية لازم يكون بعد تاريخ البداية");

        if ((toDay - fromDay).TotalDays > 366)
            throw new InvalidOperationException("الفترة المحددة طويلة جدًا (سنة كحد أقصى)");

        var orders = await _unitOfWork.Settlements.GetDeliveredOrdersAsync(fromUtc, toUtc);

        DateTime Local(DateTime utc) => utc.AddMinutes(-tzOffset);

        var data = new PeriodReportData { From = fromDay, To = toDay };

        foreach (var order in orders)
        {
            var seller = order.Items.First().Product.Seller;

            // نسبة العمولة: لو الطلب اتسوّى بنسبة التسوية، وإلا بنسبة البائع الحالية
            var percent = order.Settlement?.CommissionPercent ?? seller.CommissionPercent;
            var commission = percent.HasValue ? CommissionOf(order.TotalAmount, percent.Value) : 0;

            data.Orders.Add(new PeriodOrderRow
            {
                OrderId = order.Id,
                StoreName = seller.StoreName,
                CreatedLocal = Local(order.CreatedDate),
                DeliveredLocal = Local(order.CompletedDate ?? order.CreatedDate),
                PaymentMethod = order.PaymentMethod.MethodName,
                ItemsCount = order.Items.Sum(i => i.Quantity),
                Amount = order.TotalAmount,
                CommissionPercent = percent,
                Commission = commission,
                Net = order.TotalAmount - commission,
                Status = order.SettlementId.HasValue ? $"تمت تسويته #{order.SettlementId}" : "غير مسوّى"
            });
        }

        foreach (var group in orders.GroupBy(o => o.Items.First().Product.Seller.Id))
        {
            var seller = group.First().Items.First().Product.Seller;
            var sellerRows = data.Orders.Where(o => group.Any(g => g.Id == o.OrderId)).ToList();

            data.Sellers.Add(new PeriodSellerRow
            {
                StoreName = seller.StoreName,
                OwnerName = $"{seller.User.FirstName} {seller.User.LastName}".Trim(),
                OwnerEmail = seller.User.Email ?? string.Empty,
                OwnerPhone = seller.User.PhoneNumber,
                PayoutMethod = seller.PayoutMethod?.ToString() ?? string.Empty,
                PayoutAccount = seller.PayoutAccount ?? string.Empty,
                CommissionPercent = seller.CommissionPercent,
                OrdersCount = sellerRows.Count,
                GrossAmount = sellerRows.Sum(o => o.Amount),
                CommissionAmount = sellerRows.Sum(o => o.Commission),
                NetAmount = sellerRows.Sum(o => o.Net),
                UnsettledNet = sellerRows.Where(o => o.Status == "غير مسوّى").Sum(o => o.Net),
                Note = seller.CommissionPercent is null ? "نسبة العمولة غير محددة (محسوبة بصفر)" : string.Empty
            });
        }

        data.Sellers = data.Sellers.OrderByDescending(s => s.NetAmount).ToList();

        return _excel.BuildPeriodReport(data);
    }

    // ================= البائع =================

    public async Task<SellerEarningsDto> GetEarningsAsync(int sellerId)
    {
        var seller = await _unitOfWork.Sellers.GetByIdAsync(sellerId)
            ?? throw new KeyNotFoundException("البائع غير موجود");

        var rows = await _unitOfWork.Settlements.GetUnsettledOrderRowsAsync(DateTime.UtcNow, sellerId);
        var (gross, commission) = Calculate(rows, seller.CommissionPercent);
        var paid = await _unitOfWork.Settlements.GetPaidTotalsAsync(sellerId);
        paid.TryGetValue(sellerId, out var paidTotal);

        return new SellerEarningsDto
        {
            CommissionPercent = seller.CommissionPercent,
            PayoutMethod = seller.PayoutMethod?.ToString(),
            PayoutAccount = seller.PayoutAccount,
            UnsettledOrdersCount = rows.Count,
            GrossAmount = gross,
            CommissionAmount = commission,
            NetAmount = gross - commission,
            TotalPaid = paidTotal?.Total ?? 0,
            SettlementsCount = paidTotal?.Count ?? 0
        };
    }

    // ================= مساعدات =================

    // العمولة بتتحسب لكل طلب بتقريب لأقرب قرش، فمجموع الطلبات دايمًا يطابق إجمالي التسوية
    private static decimal CommissionOf(decimal amount, decimal percent)
        => Math.Round(amount * percent / 100m, 2, MidpointRounding.AwayFromZero);

    private static (decimal Gross, decimal Commission) Calculate(IEnumerable<UnsettledOrderRow> rows, decimal? percent)
    {
        var list = rows.ToList();
        var gross = list.Sum(r => r.Amount);
        var commission = percent.HasValue ? list.Sum(r => CommissionOf(r.Amount, percent.Value)) : 0;

        return (gross, commission);
    }

    private static string? BlockReason(Seller seller)
    {
        if (seller.CommissionPercent is null) return "حدّد نسبة العمولة أولًا";
        if (seller.PayoutMethod is null || string.IsNullOrWhiteSpace(seller.PayoutAccount)) return "البائع لم يضف بيانات التحويل بعد";
        return null;
    }

    // اليوم المحدد بتوقيت المستخدم → لحظة UTC. endOfDay = نهاية اليوم (غير شاملة)، وإلا بدايته
    private static DateTime CutoffUtc(string value, int tzOffset, out DateTime day, bool endOfDay = true)
    {
        if (!DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            throw new InvalidOperationException("تاريخ غير صحيح");

        tzOffset = Math.Clamp(tzOffset, -840, 840);

        day = date.ToDateTime(TimeOnly.MinValue);
        var shift = TimeSpan.FromMinutes(-tzOffset);

        return (endOfDay ? day.AddDays(1) : day) - shift;
    }

    private static SettlementDto ToDto(Settlement s, string storeName) => new()
    {
        Id = s.Id,
        SellerId = s.SellerId,
        StoreName = storeName,
        CreatedAt = s.CreatedAt,
        CutoffDay = s.CutoffDay,
        OrdersCount = s.OrdersCount,
        GrossAmount = s.GrossAmount,
        CommissionPercent = s.CommissionPercent,
        CommissionAmount = s.CommissionAmount,
        AdjustmentAmount = s.AdjustmentAmount,
        AdjustmentNote = s.AdjustmentNote,
        NetAmount = s.NetAmount,
        PayoutMethod = s.PayoutMethod,
        PayoutAccount = s.PayoutAccount,
        Reference = s.Reference
    };

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