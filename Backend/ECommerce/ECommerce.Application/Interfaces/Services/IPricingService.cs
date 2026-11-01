using ECommerce.Application.Pricing;

namespace ECommerce.Application.Interfaces.Services;

public interface IPricingService
{
    Task<PriceBook> LoadAsync();
}