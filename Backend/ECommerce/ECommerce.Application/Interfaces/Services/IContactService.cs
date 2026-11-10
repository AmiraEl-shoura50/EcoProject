using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Contact;

namespace ECommerce.Application.Interfaces.Services;

public interface IContactService
{
    Task SubmitAsync(ContactMessageDto dto);
    Task<PaginatedResultDto<ContactMessageItemDto>> GetAllAsync(string? filter, string? search, int pageNumber, int pageSize);
    Task<int> GetUnreadCountAsync();
    Task<bool> SetReadAsync(int id, bool isRead);
    Task<int> MarkAllAsReadAsync();
    Task<bool> DeleteAsync(int id);
}