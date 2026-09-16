using AutoMapper;
using ECommerce.Application.DTOs.Notification;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public NotificationService(IUnitOfWork unitOfWork, IMapper mapper, IRealtimeNotifier realtimeNotifier)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task CreateAndSendAsync(Guid userId, string title, string message)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<NotificationDto>(notification);
        await _realtimeNotifier.NotifyUserAsync(userId, dto); // ✅ يوصل فورًا لو المستخدم Online
    }

    public async Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(Guid userId)
    {
        var notifications = await _unitOfWork.Notifications.FindAsync(n => n.UserId == userId);
        var ordered = notifications.OrderByDescending(n => n.CreatedAt);
        return _mapper.Map<IEnumerable<NotificationDto>>(ordered);
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, Guid userId)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
        if (notification is null || notification.UserId != userId) return false;

        notification.IsRead = true;
        _unitOfWork.Notifications.Update(notification);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}