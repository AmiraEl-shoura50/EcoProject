using AutoMapper;
using ECommerce.Application.DTOs.Category;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
    }

    //public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    //{
    //    var categories = await _unitOfWork.Categories.GetAllAsync();
    //    return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    //}
    public async Task<PaginatedResultDto<CategoryDto>> SearchAsync(CategoryQueryParams queryParams)
    {
        var (items, totalCount) = await _unitOfWork.Categories.SearchAsync(queryParams);

        return new PaginatedResultDto<CategoryDto>
        {
            Items = _mapper.Map<List<CategoryDto>>(items),
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        return category is null ? null : _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var category = _mapper.Map<Category>(dto);

        if (dto.Image is not null)
        {
            using var stream = dto.Image.OpenReadStream();
            category.ImageUrl = await _fileStorageService.UploadImageAsync(
                stream, dto.Image.FileName, "categories");
        }

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<bool> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category is null) return false;

        if (dto.Image is not null)
        {
            if (!string.IsNullOrEmpty(category.ImageUrl))
            {
                var oldPublicId = _fileStorageService.ExtractPublicId(category.ImageUrl);
                await _fileStorageService.DeleteImageAsync(oldPublicId);
            }

            using var stream = dto.Image.OpenReadStream();
            category.ImageUrl = await _fileStorageService.UploadImageAsync(
                stream, dto.Image.FileName, "categories");
        }

        category.Name = dto.Name;
        category.Description = dto.Description;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category is null) return false;

        if (!string.IsNullOrEmpty(category.ImageUrl))
        {
            var publicId = _fileStorageService.ExtractPublicId(category.ImageUrl);
            await _fileStorageService.DeleteImageAsync(publicId);
        }

        _unitOfWork.Categories.Delete(category);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}