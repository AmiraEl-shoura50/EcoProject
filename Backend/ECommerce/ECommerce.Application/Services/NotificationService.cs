using AutoMapper;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Notification;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IRealtimeNotifier _realtimeNotifier;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IRealtimeNotifier realtimeNotifier,
        ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _realtimeNotifier = realtimeNotifier;
        _logger = logger;
    }

    public async Task CreateAndSendAsync(Guid userId, string title, string message, string? link = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Link = link,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<NotificationDto>(notification);

        // البث اللحظي بأقصى جهد: الإشعار اتحفظ خلاص، وفشل البث ميكسرش العملية اللي ناديتنا (طلب، دفع...)
        try
        {
            await _realtimeNotifier.NotifyUserAsync(userId, dto);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "تعذر بث الإشعار اللحظي للمستخدم {UserId}", userId);
        }
    }

    public async Task<PaginatedResultDto<NotificationDto>> GetMyNotificationsAsync(Guid userId, int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, totalCount) = await _unitOfWork.Notifications.GetByUserPagedAsync(userId, pageNumber, pageSize);

        return new PaginatedResultDto<NotificationDto>
        {
            Items = _mapper.Map<List<NotificationDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public Task<int> GetUnreadCountAsync(Guid userId)
        => _unitOfWork.Notifications.CountUnreadAsync(userId);

    public async Task<bool> MarkAsReadAsync(int notificationId, Guid userId)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
        if (notification is null || notification.UserId != userId) return false;

        notification.IsRead = true;
        _unitOfWork.Notifications.Update(notification);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public Task<int> MarkAllAsReadAsync(Guid userId)
        => _unitOfWork.Notifications.MarkAllAsReadAsync(userId);

    public async Task<bool> DeleteAsync(int notificationId, Guid userId)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
        if (notification is null || notification.UserId != userId) return false;

        _unitOfWork.Notifications.Delete(notification);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public Task<int> DeleteReadAsync(Guid userId)
        => _unitOfWork.Notifications.DeleteReadAsync(userId);
}