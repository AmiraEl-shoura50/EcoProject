export interface Seller {
  id: number;
  storeName: string;
  imageUrl: string | null;
  rating: number;
  reviewsCount: number;
  productsCount: number;
}

export interface SellerProfile extends Seller {
  firstName: string;
  lastName: string;
  email: string;
}