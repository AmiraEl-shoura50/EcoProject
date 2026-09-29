import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ProductService } from '../../../core/services/product';
import { CategoryService } from '../../../core/services/category';
import { PaginatedResult, Product } from '../../../core/models/product.model';
import { ProductCard } from '../../../shared/components/product-card/product-card';

@Component({
  selector: 'app-category-products',
  standalone: true,
  imports: [CommonModule, ProductCard],
  templateUrl: './category-products.html',
  styleUrl: './category-products.css'
})
export class CategoryProducts implements OnInit {
  products = signal<Product[]>([]);
  isLoading = signal<boolean>(true);
  categoryName = signal<string>('');
  skeletonItems = [1, 2, 3, 4, 5, 6, 7, 8];

  // ✅ جديد - حالة الـ Pagination
  currentPage = signal<number>(1);
  totalPages = signal<number>(1);
  hasPreviousPage = signal<boolean>(false);
  hasNextPage = signal<boolean>(false);

  private readonly pageSize = 12;
  private categoryId!: number;

  constructor(
    private route: ActivatedRoute,
    private productService: ProductService,
    private categoryService: CategoryService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.categoryId = +params['id'];
      this.currentPage.set(1); // ✅ لما نغيّر القسم، نرجع لأول صفحة
      this.loadCategoryInfo();
      this.loadProducts();
    });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;

    this.currentPage.set(page);
    this.loadProducts();

    // ✅ يرجّع المستخدم لأعلى الصفحة عشان يشوف المنتجات الجديدة على طول
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  private loadCategoryInfo(): void {
    this.categoryService.getById(this.categoryId).subscribe({
      next: (response) => {
        this.categoryName.set(response.data.name);
      }
    });
  }

  private loadProducts(): void {
    this.isLoading.set(true);

    this.productService.search({
      categoryId: this.categoryId,
      pageNumber: this.currentPage(),
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        this.applyResult(response.data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  private applyResult(result: PaginatedResult<Product>): void {
    this.products.set(result.items);
    this.totalPages.set(result.totalPages);
    this.hasPreviousPage.set(result.hasPreviousPage);
    this.hasNextPage.set(result.hasNextPage);
  }
}