using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    ICustomerRepository Customers { get; }
    ISellerRepository Sellers { get; }
    ICartRepository Carts { get; }
    IGenericRepository<CartItem> CartItems { get; }
    IWishlistRepository Wishlists { get; }
    IGenericRepository<WishlistItem> WishlistItems { get; }
    IGenericRepository<PaymentMethod> PaymentMethods { get; }
   
    IGenericRepository<DiscountTarget> DiscountTargets { get; }
    IDiscountRepository Discounts { get; }

    ICategoryRepository Categories { get; }

    IReviewRepository Reviews { get; }

    INotificationRepository Notifications { get; }
    IGenericRepository<PaymentProof> PaymentProofs { get; }
    IGenericRepository<PaymentTransaction> PaymentTransactions { get; }
    IPaymentGroupRepository PaymentGroups { get; }
    IDashboardRepository Dashboard { get; }
    IContactMessageRepository ContactMessages { get; }
    ISettlementRepository Settlements { get; }

    IGenericRepository<RefreshToken> RefreshTokens { get; }
    IProductImageRepository ProductImages { get; }

    Task<int> SaveChangesAsync();
}