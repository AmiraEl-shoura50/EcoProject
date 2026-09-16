using AutoMapper;
using ECommerce.Application.DTOs.Notification;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings;

public class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        CreateMap<Notification, NotificationDto>();
    }
}