export interface PaymentMethod {
  id: number;
  methodName: string;
  type: 'Manual' | 'Gateway';
}