export type DiscountType = 'Percentage' | 'FixedAmount';
export type DiscountTargetType = 'Product' | 'Category';

export interface Discount {
  id: number;
  discountType: DiscountType;
  discountAmount: number;
  startDate: string;
  endDate: string;
  targetType: DiscountTargetType;
  targetId: number;
  targetName: string;
}

export interface CreateDiscountRequest {
  discountType: DiscountType;
  discountAmount: number;
  startDate: string;
  endDate: string;
  targetType: DiscountTargetType;
  targetId: number;
}