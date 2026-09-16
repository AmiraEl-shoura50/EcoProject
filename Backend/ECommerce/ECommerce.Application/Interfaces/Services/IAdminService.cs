using ECommerce.Application.DTOs.Admin;

namespace ECommerce.Application.Interfaces.Services;

public interface IAdminService
{
    Task<IEnumerable<UserSummaryDto>> GetAllUsersAsync(string? role = null);
    Task<UserSummaryDto?> GetUserByIdAsync(Guid userId);
    Task<bool> SetUserActiveStatusAsync(Guid userId, bool isActive);
}