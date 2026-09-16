using ECommerce.Application.DTOs.Notification;

namespace ECommerce.Application.Interfaces.Services;

public interface IRealtimeNotifier
{
    Task NotifyUserAsync(Guid userId, NotificationDto notification);
}