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
  imageUrl: string | null;
}

export interface ProductQueryParams {
  searchTerm?: string;
  categoryId?: number;
  minPrice?: number;
  maxPrice?: number;
  sortBy?: string;
  pageNumber?: number;
  pageSize?: number;
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