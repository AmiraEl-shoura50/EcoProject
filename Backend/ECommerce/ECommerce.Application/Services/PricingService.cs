using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Pricing;

namespace ECommerce.Application.Services;

public class PricingService : IPricingService
{
    private readonly IUnitOfWork _unitOfWork;

    public PricingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PriceBook> LoadAsync()
    {
        var discounts = await _unitOfWork.Discounts.GetActiveAsync();
        var categories = await _unitOfWork.Categories.GetAllAsync();

        var parents = categories.ToDictionary(c => c.Id, c => c.ParentCategoryId);

        return new PriceBook(discounts, parents);
    }
}