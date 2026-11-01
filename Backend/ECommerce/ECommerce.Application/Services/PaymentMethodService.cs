using AutoMapper;
using ECommerce.Application.DTOs.PaymentMethod;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Services;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PaymentMethodService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // ✅ العملاء بيشوفوا الطرق الظاهرة بس
    public async Task<IEnumerable<PaymentMethodDto>> GetAllAsync()
    {
        var methods = await _unitOfWork.PaymentMethods.GetAllAsync();
        return _mapper.Map<IEnumerable<PaymentMethodDto>>(methods.Where(m => !m.IsHidden));
    }

    public async Task<IEnumerable<AdminPaymentMethodDto>> GetAllForAdminAsync()
    {
        var methods = await _unitOfWork.PaymentMethods.GetAllAsync();
        var counts = await _unitOfWork.Orders.CountByPaymentMethodAsync();

        return methods
            .OrderBy(m => m.Id)
            .Select(m => new AdminPaymentMethodDto
            {
                Id = m.Id,
                MethodName = m.MethodName,
                Type = m.Type.ToString(),
                ProviderIntegrationId = m.ProviderIntegrationId,
                TransferUrl = m.TransferUrl,
                IsHidden = m.IsHidden,
                OrdersCount = counts.GetValueOrDefault(m.Id)
            })
            .ToList();
    }

    public async Task<PaymentMethodDto> CreateAsync(CreatePaymentMethodDto dto)
    {
        var name = dto.MethodName.Trim();
        await EnsureUniqueNameAsync(name, null);

        var method = _mapper.Map<PaymentMethod>(dto);
        method.MethodName = name;
        method.ProviderIntegrationId = dto.Type == PaymentMethodType.Gateway
            ? dto.ProviderIntegrationId!.Trim()
            : null;
        method.TransferUrl = dto.Type == PaymentMethodType.Manual
            ? NormalizeUrl(dto.TransferUrl)
            : null;

        await _unitOfWork.PaymentMethods.AddAsync(method);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PaymentMethodDto>(method);
    }

    public async Task<bool> UpdateAsync(int id, UpdatePaymentMethodDto dto)
    {
        var method = await _unitOfWork.PaymentMethods.GetByIdAsync(id);
        if (method is null) return false;

        var name = dto.MethodName.Trim();
        await EnsureUniqueNameAsync(name, id);

        // نوع الطريقة مبيتغيّرش بعد الإنشاء عشان الطلبات القديمة متتلخبطش
        if (method.Type == PaymentMethodType.Gateway)
        {
            var integrationId = dto.ProviderIntegrationId?.Trim();

            if (string.IsNullOrEmpty(integrationId) || !integrationId.All(c => c is >= '0' and <= '9'))
                throw new InvalidOperationException("رقم التكامل مع بوابة الدفع مطلوب ويجب أن يحتوي على أرقام إنجليزية فقط");

            method.ProviderIntegrationId = integrationId;
        }
        else
        {
            method.TransferUrl = NormalizeUrl(dto.TransferUrl);
        }

        method.MethodName = name;

        _unitOfWork.PaymentMethods.Update(method);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SetVisibilityAsync(int id, bool isHidden)
    {
        var method = await _unitOfWork.PaymentMethods.GetByIdAsync(id);
        if (method is null) return false;

        method.IsHidden = isHidden;

        _unitOfWork.PaymentMethods.Update(method);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<DeletePaymentMethodResult> DeleteAsync(int id)
    {
        var method = await _unitOfWork.PaymentMethods.GetByIdAsync(id);
        if (method is null) return DeletePaymentMethodResult.NotFound;

        var counts = await _unitOfWork.Orders.CountByPaymentMethodAsync();
        if (counts.GetValueOrDefault(id) > 0) return DeletePaymentMethodResult.HasOrders;

        _unitOfWork.PaymentMethods.Delete(method);
        await _unitOfWork.SaveChangesAsync();

        return DeletePaymentMethodResult.Deleted;
    }

    private static string? NormalizeUrl(string? url)
        => string.IsNullOrWhiteSpace(url) ? null : url.Trim();

    private async Task EnsureUniqueNameAsync(string name, int? exceptId)
    {
        var all = await _unitOfWork.PaymentMethods.GetAllAsync();

        var exists = all.Any(m =>
            m.Id != exceptId &&
            string.Equals(m.MethodName.Trim(), name, StringComparison.OrdinalIgnoreCase));

        if (exists)
            throw new InvalidOperationException("يوجد طريقة دفع بنفس الاسم بالفعل");
    }
}