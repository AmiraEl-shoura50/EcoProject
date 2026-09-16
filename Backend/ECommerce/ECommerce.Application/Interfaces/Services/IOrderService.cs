using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Order;

namespace ECommerce.Application.Interfaces.Services;

public interface IOrderService
{
    Task<IEnumerable<CheckoutResultDto>> CheckoutAsync(int customerId, CreateOrderDto dto);
    Task<PaginatedResultDto<OrderDto>> GetMyOrdersAsync(int customerId, int pageNumber, int pageSize);
    Task<OrderDto?> GetByIdAsync(int orderId, int customerId);
    Task<bool> CancelAsync(int orderId, int customerId);
    Task<IEnumerable<OrderDto>> GetOrdersForSellerAsync(int sellerId);
    Task<bool> UpdateStatusAsync(int orderId, int sellerId, string newStatus);

    Task<PaymentProofDto?> SubmitPaymentProofAsync(int customerId, int orderId, SubmitPaymentProofDto dto);
    Task<bool> ReviewPaymentProofAsync(int sellerId, int orderId, ReviewPaymentProofDto dto);
}