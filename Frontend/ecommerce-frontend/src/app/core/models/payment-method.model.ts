export type PaymentMethodType = 'Manual' | 'Gateway';

export interface PaymentMethod {
  id: number;
  methodName: string;
  type: PaymentMethodType;
}

export interface AdminPaymentMethod {
  id: number;
  methodName: string;
  type: PaymentMethodType;
  providerIntegrationId: string | null;
  transferUrl: string | null;
  isHidden: boolean;
  ordersCount: number;
}

export interface CreatePaymentMethodRequest {
  methodName: string;
  type: PaymentMethodType;
  providerIntegrationId: string | null;
  transferUrl: string | null;
}

export interface UpdatePaymentMethodRequest {
  methodName: string;
  providerIntegrationId: string | null;
  transferUrl: string | null;
}