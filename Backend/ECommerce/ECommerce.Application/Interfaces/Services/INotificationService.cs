using ECommerce.Application.DTOs.Notification;

namespace ECommerce.Application.Interfaces.Services;

public interface INotificationService
{
    Task CreateAndSendAsync(Guid userId, string title, string message);
    Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(Guid userId);
    Task<bool> MarkAsReadAsync(int notificationId, Guid userId);
}