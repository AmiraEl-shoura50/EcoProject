using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;

namespace ECommerce.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Products = new ProductRepository(_context);
        Orders = new OrderRepository(_context);
        Customers = new CustomerRepository(_context);
        Sellers = new SellerRepository(_context);
        Carts = new CartRepository(_context);
        CartItems = new GenericRepository<CartItem>(_context);
        Wishlists = new WishlistRepository(_context);
        WishlistItems = new GenericRepository<WishlistItem>(_context);
        PaymentMethods = new GenericRepository<PaymentMethod>(_context);
        Discounts = new DiscountRepository(_context);
        DiscountTargets = new GenericRepository<DiscountTarget>(_context);
        Categories = new CategoryRepository(_context);
        Reviews = new ReviewRepository(_context);
        Notifications = new GenericRepository<Notification>(_context);
        PaymentProofs = new GenericRepository<PaymentProof>(_context);
        PaymentTransactions = new GenericRepository<PaymentTransaction>(_context);
        RefreshTokens = new GenericRepository<RefreshToken>(_context);
    }

    public IProductRepository Products { get; }
    public IOrderRepository Orders { get; }
    public ICustomerRepository Customers { get; }
    public ISellerRepository Sellers { get; }
    public ICartRepository Carts { get; }
    public IGenericRepository<CartItem> CartItems { get; }
    public IWishlistRepository Wishlists { get; }
    public IGenericRepository<WishlistItem> WishlistItems { get; }
    public IGenericRepository<PaymentMethod> PaymentMethods { get; }
  
    public IDiscountRepository Discounts { get; }
    public IGenericRepository<DiscountTarget> DiscountTargets { get; }

    public ICategoryRepository Categories { get; }
    public IReviewRepository Reviews { get; }
    public IGenericRepository<Notification> Notifications { get; }
    public IGenericRepository<PaymentProof> PaymentProofs { get; }
    public IGenericRepository<PaymentTransaction> PaymentTransactions { get; }
    public IGenericRepository<RefreshToken> RefreshTokens { get; }
    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}