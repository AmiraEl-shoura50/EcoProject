export interface CartItem {
  productId: number;
  productName: string;
  productImageUrl: string | null;
  unitPrice: number;
  quantity: number;
  subtotal: number;
}

export interface Cart {
  id: number;
  items: CartItem[];
  totalPrice: number;
}

export interface AddToCartRequest {
  productId: number;
  quantity: number;
}