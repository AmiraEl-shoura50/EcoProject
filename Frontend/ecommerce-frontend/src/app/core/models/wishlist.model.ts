export interface WishlistItem {
  productId: number;
  productName: string;
  productImageUrl: string | null;
  price: number;
}

export interface Wishlist {
  id: number;
  items: WishlistItem[];
}