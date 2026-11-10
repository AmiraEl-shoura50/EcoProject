export interface DailySales {
  date: string; // yyyy-MM-dd
  amount: number;
}

export interface TopProduct {
  productId: number;
  name: string;
  imageUrl: string | null;
  quantitySold: number;
  revenue: number;
}

export interface LowStockProduct {
  productId: number;
  name: string;
  imageUrl: string | null;
  stockQuantity: number;
}

export interface TopSeller {
  sellerId: number;
  storeName: string;
  imageUrl: string | null;
  ordersCount: number;
  revenue: number;
}

export interface SellerDashboardData {
  totalSales: number;
  inProgressSales: number;
  salesLast30Days: number;
  deliveredOrders: number;

  ordersToReview: number;
  ordersToShip: number;
  ordersInTransit: number;
  ordersAwaitingPayment: number;

  productsCount: number;
  outOfStockCount: number;
  lowStockCount: number;

  rating: number;
  reviewsCount: number;

  dailySales: DailySales[];
  topProducts: TopProduct[];
  lowStockProducts: LowStockProduct[];
}

export interface AdminDashboardData {
  totalUsers: number;
  customersCount: number;
  sellersCount: number;
  suspendedUsers: number;

  productsCount: number;
  categoriesCount: number;

  totalOrders: number;
  ordersByStatus: Record<string, number>;
  totalSales: number;
  salesLast30Days: number;

  dailySales: DailySales[];
  topSellers: TopSeller[];
}