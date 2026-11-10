export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  rating: number;
  categoryId: number;
  categoryName: string;
  sellerId: number;
  sellerStoreName: string;
  reviewsCount: number;
  imageUrl: string | null;
  discountedPrice: number | null;
  discountPercent: number | null;
  discountEndDate: string | null;
}

export interface ProductQueryParams {
  searchTerm?: string;
  categoryId?: number;
  minPrice?: number;
  maxPrice?: number;
  sortBy?: string;
  pageNumber?: number;
  pageSize?: number;
  sellerId?: number;
  stockFilter?: string;
  
}

export interface PaginatedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
export interface DeleteProductsResult {
  deletedCount: number;
  blockedCount: number;
}

export interface StockSummary {
  outOfStock: number;
  lowStock: number;
}