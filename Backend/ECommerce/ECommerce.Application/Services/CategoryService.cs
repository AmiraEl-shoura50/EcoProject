using AutoMapper;
using ECommerce.Application.DTOs.Category;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
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
        if (category is null || await IsHiddenAsync(category)) return null; // المخفي بيرجع NotFound للعامة

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<IEnumerable<CategoryDto>> GetTopLevelWithChildrenAsync()
    {
        var all = (await _unitOfWork.Categories.GetAllAsync()).ToList();
        var byId = all.ToDictionary(c => c.Id);

        bool IsHidden(Category c)
            => c.IsHiddenBySeller || c.IsHiddenByAdmin ||
               (c.ParentCategoryId is { } parentId && byId.TryGetValue(parentId, out var parent) &&
                (parent.IsHiddenBySeller || parent.IsHiddenByAdmin));

        var visible = all.Where(c => !IsHidden(c)).ToList();

        var topLevel = visible.Where(c => c.ParentCategoryId is null);
        var dtos = _mapper.Map<List<CategoryDto>>(topLevel);

        foreach (var dto in dtos)
        {
            var children = visible.Where(c => c.ParentCategoryId == dto.Id);
            dto.SubCategories = _mapper.Map<List<CategoryDto>>(children);
        }

        return dtos;
    }

    private async Task<bool> IsHiddenAsync(Category category)
    {
        if (category.IsHiddenBySeller || category.IsHiddenByAdmin) return true;
        if (category.ParentCategoryId is null) return false;

        var parent = await _unitOfWork.Categories.GetByIdAsync(category.ParentCategoryId.Value);
        return parent is not null && (parent.IsHiddenBySeller || parent.IsHiddenByAdmin);
    }

    public async Task<IEnumerable<SellerCategoryDto>> GetMineAsync(int sellerId)
        => await _unitOfWork.Categories.GetBySellerAsync(sellerId);

    public async Task<CategoryDto?> CreateAsync(CreateCategoryDto dto, string userRole, int? sellerId)
    {
        // ✅ الأدمن بس يقدر يعمل قسم رئيسي
        if (dto.ParentCategoryId is null && userRole != Roles.Admin)
            return null;

        // ✅ لو فيه ParentCategoryId، لازم يكون حقيقي وقسم رئيسي فعلاً (مش فرعي تاني)
        if (dto.ParentCategoryId is not null)
        {
            var parent = await _unitOfWork.Categories.GetByIdAsync(dto.ParentCategoryId.Value);
            if (parent is null || parent.ParentCategoryId is not null)
                return null;
        }

        // ✅ منع تكرار الاسم داخل نفس المستوى
        var siblings = await _unitOfWork.Categories.FindAsync(c => c.ParentCategoryId == dto.ParentCategoryId);
        if (siblings.Any(c => SameName(c.Name, dto.Name)))
            throw new InvalidOperationException("يوجد قسم بنفس الاسم بالفعل في هذا المستوى");

        var category = _mapper.Map<Category>(dto);
        category.Name = dto.Name.Trim();
        category.Description = dto.Description.Trim();
        category.CreatedBySellerId = userRole == Roles.Seller ? sellerId : null;

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

    public async Task<bool> UpdateAsync(int id, UpdateCategoryDto dto, string userRole, int? sellerId)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category is null) return false;

        if (!CanManage(category, userRole, sellerId)) return false;

        var siblings = await _unitOfWork.Categories.FindAsync(c =>
            c.ParentCategoryId == category.ParentCategoryId && c.Id != id);
        if (siblings.Any(c => SameName(c.Name, dto.Name)))
            throw new InvalidOperationException("يوجد قسم بنفس الاسم بالفعل في هذا المستوى");

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

        category.Name = dto.Name.Trim();
        category.Description = dto.Description.Trim();

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<DeleteCategoryResult> DeleteAsync(int id, string userRole, int? sellerId)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category is null || !CanManage(category, userRole, sellerId))
            return DeleteCategoryResult.NotFound;

        // ✅ العلاقات Restrict، فلازم نتأكد قبل الحذف بدل ما ييجي خطأ 500
        var hasSubCategories = (await _unitOfWork.Categories.FindAsync(c => c.ParentCategoryId == id)).Any();
        if (hasSubCategories) return DeleteCategoryResult.HasSubCategories;

        var hasProducts = (await _unitOfWork.Products.FindAsync(p => p.CategoryId == id)).Any();
        if (hasProducts) return DeleteCategoryResult.HasProducts;

        var imageUrl = category.ImageUrl;

        _unitOfWork.Categories.Delete(category);
        await _unitOfWork.SaveChangesAsync();

        // نمسح الصورة بعد نجاح الحذف مش قبله
        if (!string.IsNullOrEmpty(imageUrl))
        {
            var publicId = _fileStorageService.ExtractPublicId(imageUrl);
            await _fileStorageService.DeleteImageAsync(publicId);
        }

        return DeleteCategoryResult.Deleted;
    }

    // الأدمن يدير أي قسم، والبائع يدير بس الأقسام اللي أنشأها هو
    private static bool CanManage(Category category, string userRole, int? sellerId)
    {
        if (userRole == Roles.Admin) return true;

        // ⚠️ لازم نتأكد إن sellerId مش null، وإلا null == null هتسمح بتعديل أقسام الأدمن
        return sellerId is not null && category.CreatedBySellerId == sellerId;
    }

    private static bool SameName(string a, string b)
        => string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);

    public async Task<IEnumerable<AdminCategoryDto>> GetAdminTreeAsync()
    => await _unitOfWork.Categories.GetAdminTreeAsync();
}