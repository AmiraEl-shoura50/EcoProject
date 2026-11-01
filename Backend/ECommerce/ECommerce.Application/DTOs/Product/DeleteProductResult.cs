namespace ECommerce.Application.DTOs.Product;

public enum DeleteProductResult
{
    Deleted,
    NotFound,
    HasOrders
}