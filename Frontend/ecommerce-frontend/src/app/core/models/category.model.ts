export interface Category {
  id: number;
  name: string;
  description: string;
  imageUrl: string | null;
  parentCategoryId: number | null;
  subCategories: Category[];
}
