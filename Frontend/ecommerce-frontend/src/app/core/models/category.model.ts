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
}

export interface AdminSubCategory {
  id: number;
  name: string;
  productsCount: number;
  storeName: string | null;
}

export interface AdminCategory {
  id: number;
  name: string;
  description: string;
  imageUrl: string | null;
  productsCount: number;
  subCategories: AdminSubCategory[];
}