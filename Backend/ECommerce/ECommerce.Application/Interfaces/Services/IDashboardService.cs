using ECommerce.Application.DTOs.Dashboard;

namespace ECommerce.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<SellerDashboardDto> GetSellerAsync(int sellerId, int tzOffsetMinutes);
    Task<AdminDashboardDto> GetAdminAsync(int tzOffsetMinutes);
}