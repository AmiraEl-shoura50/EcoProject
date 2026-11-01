export interface LatestReview {
  customerName: string;
  productName: string;
  rating: number;
  comment: string | null;
  createdAt: string;
}

export interface Review {
  id: number;
  productId: number;
  customerName: string;
  rating: number;
  comment: string | null;
  createdAt: string;
}

export interface ReviewSummary {
  average: number;
  totalCount: number;
  distribution: number[]; // distribution[0] = نجمة واحدة ... distribution[4] = 5 نجوم
}

export interface CreateReviewRequest {
  productId: number;
  orderId: number;
  rating: number;
  comment: string | null;
}