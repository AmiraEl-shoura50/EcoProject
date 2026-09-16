using ECommerce.API.Hubs;
using ECommerce.Application.DTOs.Notification;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.API.Realtime;

public class SignalRNotifier : IRealtimeNotifier
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotifier(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyUserAsync(Guid userId, NotificationDto notification)
    {
        await _hubContext.Clients.Group(userId.ToString())
            .SendAsync("ReceiveNotification", notification);
    }
}