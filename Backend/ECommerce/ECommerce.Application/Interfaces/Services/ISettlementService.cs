using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Settlement;

namespace ECommerce.Application.Interfaces.Services;

public interface ISettlementService
{
    // الأدمن
    Task<List<SellerBalanceDto>> GetBalancesAsync(string cutoff, int tzOffset);
    Task<bool> SetCommissionAsync(int sellerId, decimal percent);
    Task<SettlementDto> CreateAsync(int sellerId, CreateSettlementDto dto, Guid adminUserId);
    Task<PaginatedResultDto<SettlementDto>> GetHistoryAsync(int? sellerId, int pageNumber, int pageSize);
    Task<byte[]?> ExportSettlementAsync(int settlementId, int? sellerId, int tzOffset);
    Task<byte[]> ExportPeriodReportAsync(string from, string to, int tzOffset);

    // البائع
    Task<SellerEarningsDto> GetEarningsAsync(int sellerId);
}