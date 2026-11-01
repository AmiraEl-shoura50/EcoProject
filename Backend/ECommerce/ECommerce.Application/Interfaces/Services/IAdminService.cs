using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.DTOs.Common;

namespace ECommerce.Application.Interfaces.Services;

public interface IAdminService
{
    Task<PaginatedResultDto<UserSummaryDto>> GetUsersAsync(string? role, string? search, int pageNumber, int pageSize);
    Task<UserSummaryDto?> GetUserByIdAsync(Guid userId);
    Task<bool> SetUserActiveStatusAsync(Guid userId, bool isActive);
}