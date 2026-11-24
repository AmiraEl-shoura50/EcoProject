export interface SellerBalance {
  sellerId: number;
  storeName: string;
  imageUrl: string | null;
  ownerName: string;
  ownerEmail: string;
  ownerPhone: string | null;
  commissionPercent: number | null;
  payoutMethod: string | null;
  payoutAccount: string | null;
  ordersCount: number;
  grossAmount: number;
  commissionAmount: number;
  netAmount: number;
  totalPaid: number;
  settlementsCount: number;
  blockReason: string | null;
}

export interface Settlement {
  id: number;
  sellerId: number;
  storeName: string;
  createdAt: string;
  cutoffDay: string;
  ordersCount: number;
  grossAmount: number;
  commissionPercent: number;
  commissionAmount: number;
  adjustmentAmount: number;
  adjustmentNote: string | null;
  netAmount: number;
  payoutMethod: string;
  payoutAccount: string;
  reference: string | null;
}

export interface CreateSettlementRequest {
  cutoff: string;
  expectedOrdersCount: number;
  adjustmentAmount: number;
  adjustmentNote: string | null;
  reference: string | null;
}

export interface SellerEarnings {
  commissionPercent: number | null;
  payoutMethod: string | null;
  payoutAccount: string | null;
  unsettledOrdersCount: number;
  grossAmount: number;
  commissionAmount: number;
  netAmount: number;
  totalPaid: number;
  settlementsCount: number;
}

export const PAYOUT_LABELS: Record<string, string> = {
  VodafoneCash: 'فودافون كاش',
  InstaPay: 'إنستاباي',
  BankAccount: 'حساب بنكي'
};