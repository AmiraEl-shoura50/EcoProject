export interface CreateOrderRequest {
  paymentMethodId: number;
}

export interface OrderItem {
  productId: number;
  productName: string;
  productImageUrl: string | null;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}


export interface Order {
  id: number;
  status: string;
  createdDate: string;
  completedDate: string | null;
  totalAmount: number;
  paymentMethodName: string;
  paymentMethodType: 'Manual' | 'Gateway';
  items: OrderItem[];
}

export interface CheckoutResult {
  order: Order;
  paymentUrl: string | null;
}
export interface PaymentProof {
  id: number;
  orderId: number;
  imageUrl: string;
  transferReference: string | null;
  submittedAt: string;
  reviewedAt: string | null;
  isApproved: boolean | null;
  rejectionReason: string | null;
}