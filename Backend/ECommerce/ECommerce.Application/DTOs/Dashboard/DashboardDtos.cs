namespace ECommerce.Application.DTOs.Dashboard;

public class DailySalesDto
{
    public string Date { get; set; } = string.Empty; // yyyy-MM-dd (نص عشان المتصفح ميحوّلش اليوم بفرق التوقيت)
    public decimal Amount { get; set; }
}

public class TopProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public class LowStockProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
}

public class TopSellerDto
{
    public int SellerId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int OrdersCount { get; set; }
    public decimal Revenue { get; set; }
}

public class SellerDashboardDto
{
    // المبيعات
    public decimal TotalSales { get; set; }          // مكتملة (تم التوصيل)
    public decimal InProgressSales { get; set; }     // مدفوعة وقيد التنفيذ (مؤكد + تم الشحن)
    public decimal SalesLast30Days { get; set; }
    public int DeliveredOrders { get; set; }

    // الطلبات
    public int OrdersToReview { get; set; }          // إثبات دفع في انتظار قرار البائع
    public int OrdersToShip { get; set; }            // مؤكدة وجاهزة للشحن
    public int OrdersInTransit { get; set; }         // تم الشحن
    public int OrdersAwaitingPayment { get; set; }   // في انتظار الدفع

    // المنتجات
    public int ProductsCount { get; set; }
    public int OutOfStockCount { get; set; }
    public int LowStockCount { get; set; }

    // المتجر
    public double Rating { get; set; }
    public int ReviewsCount { get; set; }

    public List<DailySalesDto> DailySales { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
    public List<LowStockProductDto> LowStockProducts { get; set; } = new();
}

public class AdminDashboardDto
{
    // المستخدمون
    public int TotalUsers { get; set; }
    public int CustomersCount { get; set; }
    public int SellersCount { get; set; }
    public int SuspendedUsers { get; set; }

    // المحتوى
    public int ProductsCount { get; set; }
    public int CategoriesCount { get; set; }

    // الطلبات والمبيعات
    public int TotalOrders { get; set; }
    public Dictionary<string, int> OrdersByStatus { get; set; } = new();
    public decimal TotalSales { get; set; }
    public decimal SalesLast30Days { get; set; }

    public List<DailySalesDto> DailySales { get; set; } = new();
    public List<TopSellerDto> TopSellers { get; set; } = new();
}