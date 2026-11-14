using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IProductService productService, IUnitOfWork unitOfWork)
    {
        _productService = productService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] ProductQueryParams queryParams)
    {
        queryParams.StockFilter = null;
        var result = await _productService.SearchAsync(queryParams);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null) return NotFound();
        return Ok(product);
    }

    [HttpGet("category/{categoryId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        var products = await _productService.GetByCategoryAsync(categoryId);
        return Ok(products);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Seller)]
    public async Task<IActionResult> Create([FromForm] CreateProductDto dto)
    {
        var sellerId = await GetCurrentSellerIdAsync();
        if (sellerId is null) return Forbid();

        var created = await _productService.CreateAsync(dto, sellerId.Value);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Seller)]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateProductDto dto)
    {
        var sellerId = await GetCurrentSellerIdAsync();
        if (sellerId is null) return Forbid();

        var updated = await _productService.UpdateAsync(id, dto, sellerId.Value);
        if (!updated) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Seller)]
    public async Task<IActionResult> Delete(int id)
    {
        var sellerId = await GetCurrentSellerIdAsync();
        if (sellerId is null) return Forbid();

        var result = await _productService.DeleteAsync(id, sellerId.Value);

        return result switch
        {
            DeleteProductResult.NotFound => NotFound(),
            DeleteProductResult.HasOrders => BadRequest("لا يمكن حذف منتج تم طلبه من قبل. يمكنك جعل الكمية صفرًا لإيقاف بيعه"),
            _ => NoContent()
        };
    }

    [HttpGet("mine")]
    [Authorize(Roles = Roles.Seller)]
    public async Task<IActionResult> GetMine([FromQuery] ProductQueryParams queryParams)
    {
        var sellerId = await GetCurrentSellerIdAsync();
        if (sellerId is null) return Forbid();

        queryParams.SellerId = sellerId.Value;
        var result = await _productService.SearchAsync(queryParams);
        return Ok(result);
    }
    [HttpGet("offers")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOffers([FromQuery] ProductQueryParams queryParams)
    {
        var result = await _productService.GetOffersAsync(queryParams);
        return Ok(result);
    }

    // ✅ Helper method - بتجيب الـ Seller المرتبط بالـ User الحالي من الـ Token
    private async Task<int?> GetCurrentSellerIdAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        var sellers = await _unitOfWork.Sellers.FindAsync(s => s.UserId == userId);
        var seller = sellers.FirstOrDefault();

        return seller?.Id;
    }
    [HttpGet("mine/stock-summary")]
    [Authorize(Roles = Roles.Seller)]
    public async Task<IActionResult> GetMyStockSummary()
    {
        var sellerId = await GetCurrentSellerIdAsync();
        if (sellerId is null) return Forbid();

        var summary = await _productService.GetStockSummaryAsync(sellerId.Value);
        return Ok(summary);
    }

    [HttpPut("{id:int}/stock")]
    [Authorize(Roles = Roles.Seller)]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockDto dto)
    {
        var sellerId = await GetCurrentSellerIdAsync();
        if (sellerId is null) return Forbid();

        var stock = await _productService.UpdateStockAsync(id, sellerId.Value, dto);
        if (stock is null) return NotFound();

        return Ok(new { stockQuantity = stock.Value });
    }

    [HttpGet("best-sellers")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBestSellers([FromQuery] int take = 8)
    {
        var products = await _productService.GetBestSellersAsync(take);
        return Ok(products);
    }
}