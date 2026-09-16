using ECommerce.Application.DTOs.Category;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<(List<Category> Items, int TotalCount)> SearchAsync(CategoryQueryParams queryParams);
}