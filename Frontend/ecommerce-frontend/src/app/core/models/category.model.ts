export interface Category {
  id: number;
  name: string;
  description: string;
  imageUrl: string | null;
  parentCategoryId: number | null;
  subCategories: Category[];
}
export interface SellerCategory {
  id: number;
  name: string;
  description: string;
  imageUrl: string | null;
  parentCategoryId: number | null;
  parentCategoryName: string;
  productsCount: number;
  isHiddenBySeller: boolean;
  isHiddenByAdmin: boolean;
}

export interface AdminSubCategory {
  id: number;
  name: string;
  productsCount: number;
  storeName: string | null;
  isHiddenBySeller: boolean;
  isHiddenByAdmin: boolean;
}

export interface AdminCategory {
  id: number;
  name: string;
  description: string;
  imageUrl: string | null;
  productsCount: number;
  subCategories: AdminSubCategory[];
  isHiddenBySeller: boolean;
  isHiddenByAdmin: boolean;
}