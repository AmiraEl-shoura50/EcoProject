export interface AdminProduct {
  id: number;
  name: string;
  imageUrl: string | null;
  price: number;
  stockQuantity: number;
  categoryName: string;
  sellerId: number;
  sellerStoreName: string;
  isHiddenBySeller: boolean;
  isHiddenByAdmin: boolean;
  isCategoryHidden: boolean;
}