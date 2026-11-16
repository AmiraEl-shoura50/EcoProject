export interface CartItem {
  productId: number;
  productName: string;
  productImageUrl: string | null;
  unitPrice: number;
  quantity: number;
  subtotal: number;
  originalUnitPrice: number;
}

export interface Cart {
  id: number;
  items: CartItem[];
  totalPrice: number;
  removedCount: number;
}

export interface AddToCartRequest {
  productId: number;
  quantity: number;
}