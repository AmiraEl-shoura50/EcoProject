import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { ProductService } from '../../../core/services/product';
import { Product } from '../../../core/models/product.model';
import { ProductCard } from '../../../shared/components/product-card/product-card';

@Component({
  selector: 'app-search-results',
  standalone: true,
  imports: [CommonModule, RouterLink, ProductCard],
  templateUrl: './search-results.html',
  styleUrl: './search-results.css'
})
export class SearchResults implements OnInit {
  products = signal<Product[]>([]);
  isLoading = signal<boolean>(true);
  term = signal<string>('');
  sortBy = signal<string>('newest');
  totalCount = signal<number>(0);
  skeletonItems = [1, 2, 3, 4, 5, 6, 7, 8];

  currentPage = signal<number>(1);
  totalPages = signal<number>(1);
  hasPreviousPage = signal<boolean>(false);
  hasNextPage = signal<boolean>(false);

  sortOptions = [
    { value: 'newest', label: 'الأحدث' },
    { value: 'price_asc', label: 'السعر: من الأقل للأعلى' },
    { value: 'price_desc', label: 'السعر: من الأعلى للأقل' },
    { value: 'rating', label: 'الأعلى تقييمًا' }
  ];

  private readonly pageSize = 12;
  private request?: Subscription;

  constructor(
    private route: ActivatedRoute,
    private productService: ProductService
  ) {}

  ngOnInit(): void {
    // بنتابع الرابط: لو المستخدم بحث عن حاجة تانية وهو في نفس الصفحة، النتائج بتتحدّث
    this.route.queryParamMap.subscribe(params => {
      this.term.set((params.get('q') ?? '').trim());
      this.currentPage.set(1);
      this.load();
    });
  }

  onSortChange(value: string): void {
    this.sortBy.set(value);
    this.currentPage.set(1);
    this.load();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.currentPage.set(page);
    this.load();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  private load(): void {
    this.request?.unsubscribe(); // نلغي أي طلب قديم لسه شغال

    if (!this.term()) {
      this.products.set([]);
      this.totalCount.set(0);
      this.isLoading.set(false);
      return;
    }

    this.isLoading.set(true);

    this.request = this.productService.search({
      searchTerm: this.term(),
      sortBy: this.sortBy(),
      pageNumber: this.currentPage(),
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        const result = response.data;
        this.products.set(result.items);
        this.totalCount.set(result.totalCount);
        this.totalPages.set(result.totalPages);
        this.hasPreviousPage.set(result.hasPreviousPage);
        this.hasNextPage.set(result.hasNextPage);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}