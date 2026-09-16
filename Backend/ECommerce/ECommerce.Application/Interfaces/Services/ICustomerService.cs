using ECommerce.Application.DTOs.Customer;

namespace ECommerce.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(int customerId, UpdateCustomerDto dto);
}