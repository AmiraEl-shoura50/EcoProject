using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;

    public AdminService(UserManager<ApplicationUser> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<PaginatedResultDto<UserSummaryDto>> GetUsersAsync(
        string? role, string? search, int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var userRoles = from ur in _context.UserRoles
                        join r in _context.Roles on ur.RoleId equals r.Id
                        select new { ur.UserId, RoleName = r.Name };

        var query = _context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(u => userRoles.Any(x => x.UserId == u.Id && x.RoleName == role));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term) ||
                (u.Email ?? "").ToLower().Contains(term) ||
                (u.PhoneNumber ?? "").Contains(term));
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // أدوار مستخدمي الصفحة دي كلها في استعلام واحد بدل استعلام لكل مستخدم
        var userIds = users.Select(u => u.Id).ToList();
        var roleRows = await userRoles.Where(x => userIds.Contains(x.UserId)).ToListAsync();

        var items = users.Select(user => new UserSummaryDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            Roles = roleRows.Where(x => x.UserId == user.Id && x.RoleName != null)
                            .Select(x => x.RoleName!)
                            .ToList()
        }).ToList();

        return new PaginatedResultDto<UserSummaryDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<UserSummaryDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserSummaryDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            Roles = roles.ToList()
        };
    }

    public async Task<bool> SetUserActiveStatusAsync(Guid userId, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        // حماية من إن الإدارة تتقفل على نفسها
        if (await _userManager.IsInRoleAsync(user, Roles.Admin))
            throw new InvalidOperationException("لا يمكن إيقاف حساب مدير");

        // تحديث مباشر للعلامة بس، من غير ما نشغّل فحوصات الاسم والإيميل بتاعة Identity
        await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.IsActive, isActive));

        return true;
    }
    public async Task<List<Guid>> GetAdminIdsAsync()
    {
        var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);
        return admins.Where(a => a.IsActive).Select(a => a.Id).ToList();
    }
}