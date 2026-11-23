using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.DTOs.Payment;

namespace ECommerce.Application.Interfaces.Services;

public enum PaymentReviewResult { Ok, NotFound, InvalidState }

public interface IPaymentGroupService
{
    // العميل
    Task<PaymentGroupDto?> GetForCustomerAsync(int groupId, int customerId);
    Task<List<PendingPaymentDto>> GetPendingForCustomerAsync(int customerId);
    Task<PaymentProofDto?> SubmitProofAsync(int groupId, int customerId, SubmitPaymentProofDto dto);

    // الأدمن
    Task<PaginatedResultDto<AdminPaymentGroupDto>> GetForAdminAsync(string? filter, string? search, int pageNumber, int pageSize);
    Task<int> GetAwaitingCountAsync();
    Task<PaymentReviewResult> ReviewAsync(int groupId, ReviewPaymentProofDto dto);
}