using AutoMapper;
using ECommerce.Application.DTOs.PaymentMethod;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;

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

    public async Task<IEnumerable<PaymentMethodDto>> GetAllAsync()
    {
        var methods = await _unitOfWork.PaymentMethods.GetAllAsync();
        return _mapper.Map<IEnumerable<PaymentMethodDto>>(methods);
    }

    public async Task<PaymentMethodDto> CreateAsync(CreatePaymentMethodDto dto)
    {
        var method = _mapper.Map<PaymentMethod>(dto);
        await _unitOfWork.PaymentMethods.AddAsync(method);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<PaymentMethodDto>(method);
    }
}