import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { LucideAngularModule, ChevronLeft } from 'lucide-angular';
import { ProductService } from '../../../core/services/product';
import { CategoryService } from '../../../core/services/category';
import { Product } from '../../../core/models/product.model';
import { Category } from '../../../core/models/category.model';
import { ProductCard } from '../../../shared/components/product-card/product-card';

interface CategoryContext {
  category: Category;
  parent: Category | null; // null لو القسم رئيسي
}

@Component({
  selector: 'app-category-products',
  standalone: true,
  imports: [CommonModule, RouterLink, LucideAngularModule, ProductCard],
  templateUrl: './category-products.html',
  styleUrl: './category-products.css'
})
export class CategoryProducts implements OnInit {
  readonly ChevronIcon = ChevronLeft;

  products = signal<Product[]>([]);
  isLoading = signal<boolean>(true);
  skeletonItems = [1, 2, 3, 4, 5, 6, 7, 8];

  categoryId = signal<number>(0);

  // القسم الحالي وأبوه (لو فرعي)، بيتحسبوا من الشجرة وبيتحدّثوا لما توصل
  current = computed<CategoryContext | null>(() => {
    const id = this.categoryId();

    for (const main of this.categoryService.tree()) {
      if (main.id === id) return { category: main, parent: null };

      const sub = main.subCategories.find(s => s.id === id);
      if (sub) return { category: sub, parent: main };
    }

    return null;
  });

  // الأقسام الفرعية بتظهر في صفحة القسم الرئيسي بس
  subCategories = computed(() => {
    const info = this.current();
    return info && !info.parent ? info.category.subCategories : [];
  });

  notFound = computed(() => this.categoryService.treeLoaded() && !this.current());

  currentPage = signal<number>(1);
  totalPages = signal<number>(1);
  hasPreviousPage = signal<boolean>(false);
  hasNextPage = signal<boolean>(false);

  private readonly pageSize = 12;
  private request?: Subscription;

  constructor(
    private route: ActivatedRoute,
    private productService: ProductService,
    public categoryService: CategoryService
  ) {}

  ngOnInit(): void {
    // بنتابع الـ id في الرابط: التنقل بين الأقسام بيفضل في نفس الصفحة
    this.route.params.subscribe(params => {
      this.categoryId.set(+params['id']);
      this.currentPage.set(1);
      this.loadProducts();
      window.scrollTo({ top: 0 });
    });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;

    this.currentPage.set(page);
    this.loadProducts();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  private loadProducts(): void {
    this.request?.unsubscribe(); // نلغي أي طلب قديم لسه شغال
    this.isLoading.set(true);

    this.request = this.productService.search({
      categoryId: this.categoryId(),
      pageNumber: this.currentPage(),
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        const result = response.data;
        this.products.set(result.items);
        this.totalPages.set(result.totalPages);
        this.hasPreviousPage.set(result.hasPreviousPage);
        this.hasNextPage.set(result.hasNextPage);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}