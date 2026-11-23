import { OrderItem } from './order.model';

export interface PaymentGroupOrder {
  orderId: number;
  sellerId: number;
  storeName: string;
  amount: number;
  status: string;
  items: OrderItem[];
}

export interface PaymentGroup {
  id: number;
  status: 'Pending' | 'AwaitingConfirmation' | 'Paid' | 'Cancelled';
  totalAmount: number;
  createdAt: string;
  paymentMethodName: string;
  transferUrl: string | null;
  canUploadProof: boolean;
  lastRejectionReason: string | null;
  orders: PaymentGroupOrder[];
}

export interface PaymentProofInfo {
  id: number;
  imageUrl: string;
  transferReference: string | null;
  submittedAt: string;
  reviewedAt: string | null;
  isApproved: boolean | null;
  rejectionReason: string | null;
}

export interface AdminPaymentGroup extends PaymentGroup {
  customerName: string;
  customerEmail: string;
  customerPhone: string | null;
  proofs: PaymentProofInfo[];
}

export interface PendingPayment {
  id: number;
  totalAmount: number;
  createdAt: string;
  paymentMethodName: string;
  ordersCount: number;
}