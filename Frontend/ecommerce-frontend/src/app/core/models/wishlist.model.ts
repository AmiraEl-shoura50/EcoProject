export interface WishlistItem {
  productId: number;
  productName: string;
  productImageUrl: string | null;
  price: number;
  stockQuantity: number;
  rating: number;
}

export interface Wishlist {
  id: number;
  items: WishlistItem[];
}