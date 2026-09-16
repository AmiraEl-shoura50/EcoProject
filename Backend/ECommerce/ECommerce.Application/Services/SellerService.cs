using AutoMapper;
using ECommerce.Application.DTOs.Seller;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;

namespace ECommerce.Application.Services;

public class SellerService : ISellerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;

    public SellerService(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
    }

    public async Task<SellerDto?> GetByIdAsync(int id)
    {
        var seller = await _unitOfWork.Sellers.GetWithUserAsync(id); // ✅ بدل GetByIdAsync العادية
        return seller is null ? null : _mapper.Map<SellerDto>(seller);
    }

    public async Task<bool> UpdateAsync(int sellerId, UpdateSellerDto dto)
    {
        var seller = await _unitOfWork.Sellers.GetByIdAsync(sellerId);
        if (seller is null) return false;

        if (dto.Image is not null)
        {
            if (!string.IsNullOrEmpty(seller.ImageUrl))
            {
                var oldPublicId = _fileStorageService.ExtractPublicId(seller.ImageUrl);
                await _fileStorageService.DeleteImageAsync(oldPublicId);
            }

            using var stream = dto.Image.OpenReadStream();
            seller.ImageUrl = await _fileStorageService.UploadImageAsync(
                stream, dto.Image.FileName, "sellers");
        }

        seller.StoreName = dto.StoreName;

        _unitOfWork.Sellers.Update(seller);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}