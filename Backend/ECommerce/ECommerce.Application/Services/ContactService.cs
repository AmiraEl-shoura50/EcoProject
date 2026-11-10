using System.Net;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Contact;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Settings;
using ECommerce.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Application.Services;

public class ContactService : IContactService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly IAdminService _adminService;
    private readonly ContactSettings _contact;
    private readonly EmailSettings _email;
    private readonly ILogger<ContactService> _logger;

    public ContactService(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        INotificationService notificationService,
        IAdminService adminService,
        IOptions<ContactSettings> contact,
        IOptions<EmailSettings> email,
        ILogger<ContactService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _notificationService = notificationService;
        _adminService = adminService;
        _contact = contact.Value;
        _email = email.Value;
        _logger = logger;
    }

    public async Task SubmitAsync(ContactMessageDto dto)
    {
        var message = new ContactMessage
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim(),
            // نشيل أي سطور جديدة من العنوان (حماية من حقن ترويسات الإيميل)
            Subject = dto.Subject.Trim().ReplaceLineEndings(" "),
            Message = dto.Message.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        // ✅ أهم خطوة: الرسالة بتتخزن الأول، فمهما حصل في الإيميل أو الإشعار مبتضيعش
        await _unitOfWork.ContactMessages.AddAsync(message);
        await _unitOfWork.SaveChangesAsync();

        await NotifyAdminsAsync(message);
        SendSupportEmail(message);
    }

    public async Task<PaginatedResultDto<ContactMessageItemDto>> GetAllAsync(
        string? filter, string? search, int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var unreadOnly = string.Equals(filter, "unread", StringComparison.OrdinalIgnoreCase);

        var (items, totalCount) = await _unitOfWork.ContactMessages.SearchAsync(search, unreadOnly, pageNumber, pageSize);

        return new PaginatedResultDto<ContactMessageItemDto>
        {
            Items = items.Select(ToDto).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public Task<int> GetUnreadCountAsync()
        => _unitOfWork.ContactMessages.CountUnreadAsync();

    public async Task<bool> SetReadAsync(int id, bool isRead)
    {
        var message = await _unitOfWork.ContactMessages.GetByIdAsync(id);
        if (message is null) return false;

        message.IsRead = isRead;

        _unitOfWork.ContactMessages.Update(message);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public Task<int> MarkAllAsReadAsync()
        => _unitOfWork.ContactMessages.MarkAllAsReadAsync();

    public async Task<bool> DeleteAsync(int id)
    {
        var message = await _unitOfWork.ContactMessages.GetByIdAsync(id);
        if (message is null) return false;

        _unitOfWork.ContactMessages.Delete(message);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // إشعار الأدمنز على الجرس. بأقصى جهد: الرسالة اتحفظت، وفشل الإشعار ميكسرش الإرسال
    private async Task NotifyAdminsAsync(ContactMessage message)
    {
        try
        {
            var adminIds = await _adminService.GetAdminIdsAsync();

            foreach (var adminId in adminIds)
            {
                await _notificationService.CreateAndSendAsync(
                    adminId,
                    "رسالة تواصل جديدة 📩",
                    $"{message.Name}: {Truncate(message.Subject, 60)}",
                    "/admin/messages");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "تعذر إرسال إشعار رسالة التواصل للأدمن");
        }
    }

    // نسخة على بريد الدعم (إضافية، والأصل في الداتابيز). كل مدخلات المستخدم بتتحول لنص آمن قبل HTML الإيميل
    private void SendSupportEmail(ContactMessage message)
    {
        var recipient = string.IsNullOrWhiteSpace(_contact.SupportEmail)
            ? _email.SenderEmail
            : _contact.SupportEmail;

        if (string.IsNullOrWhiteSpace(recipient)) return;

        static string Encode(string value) => WebUtility.HtmlEncode(value);

        var body =
            "<h3>رسالة جديدة من نموذج التواصل</h3>" +
            $"<p><b>الاسم:</b> {Encode(message.Name)}</p>" +
            $"<p><b>البريد:</b> {Encode(message.Email)}</p>" +
            $"<p><b>الموضوع:</b> {Encode(message.Subject)}</p>" +
            "<hr/>" +
            $"<p style=\"white-space:pre-line\">{Encode(message.Message)}</p>";

        _ = _emailService.SendEmailAsync(recipient, $"[Smart Shop] {message.Subject}", body);
    }

    private static ContactMessageItemDto ToDto(ContactMessage m) => new()
    {
        Id = m.Id,
        Name = m.Name,
        Email = m.Email,
        Subject = m.Subject,
        Message = m.Message,
        CreatedAt = m.CreatedAt,
        IsRead = m.IsRead
    };

    private static string Truncate(string text, int max)
        => text.Length <= max ? text : text[..max].TrimEnd() + "…";
}