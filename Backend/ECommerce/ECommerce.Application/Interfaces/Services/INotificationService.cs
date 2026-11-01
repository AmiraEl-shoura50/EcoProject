using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Notification;

namespace ECommerce.Application.Interfaces.Services;

public interface INotificationService
{
    Task CreateAndSendAsync(Guid userId, string title, string message);
    Task<PaginatedResultDto<NotificationDto>> GetMyNotificationsAsync(Guid userId, int pageNumber, int pageSize);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<bool> MarkAsReadAsync(int notificationId, Guid userId);
    Task<int> MarkAllAsReadAsync(Guid userId);
    Task<bool> DeleteAsync(int notificationId, Guid userId);
    Task<int> DeleteReadAsync(Guid userId);
}