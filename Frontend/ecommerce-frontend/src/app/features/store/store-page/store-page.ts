import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, Subscription, debounceTime, distinctUntilChanged, map } from 'rxjs';
import { LucideAngularModule, Search, X } from 'lucide-angular';
import { SellerService } from '../../../core/services/seller';
import { ProductService } from '../../../core/services/product';
import { Seller } from '../../../core/models/seller.model';
import { Product } from '../../../core/models/product.model';
import { ProductCard } from '../../../shared/components/product-card/product-card';
import { Stars } from '../../../shared/components/stars/stars';

@Component({
  selector: 'app-store-page',
  standalone: true,
  imports: [CommonModule, RouterLink, LucideAngularModule, ProductCard, Stars],
  templateUrl: './store-page.html',
  styleUrl: './store-page.css'
})
export class StorePage implements OnInit {
  readonly SearchIcon = Search;
  readonly CloseIcon = X;

  seller = signal<Seller | null>(null);
  sellerLoaded = signal<boolean>(false);
  notFound = signal<boolean>(false);

  products = signal<Product[]>([]);
  isLoading = signal<boolean>(true);
  totalCount = signal<number>(0);
  skeletonItems = [1, 2, 3, 4, 5, 6, 7, 8];

  searchTerm = signal<string>('');
  sortBy = signal<string>('newest');

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

  private sellerId = 0;
  private readonly pageSize = 12;
  private search$ = new Subject<string>();
  private request?: Subscription;
  private destroyRef = inject(DestroyRef);

  constructor(
    private route: ActivatedRoute,
    private sellerService: SellerService,
    private productService: ProductService
  ) {}

  ngOnInit(): void {
    // بحث مباشر بعد 300ms من آخر حرف
    this.search$
      .pipe(
        map(value => value.trim()),
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.currentPage.set(1);
        this.loadProducts();
      });

    this.route.params.subscribe(params => {
      this.sellerId = +params['id'];

      this.searchTerm.set('');
      this.sortBy.set('newest');
      this.currentPage.set(1);
      this.notFound.set(false);
      window.scrollTo({ top: 0 });

      this.loadSeller();
      this.loadProducts();
    });
  }

  onSearchInput(value: string): void {
    this.searchTerm.set(value);
    this.search$.next(value);
  }

  clearSearch(): void {
    this.searchTerm.set('');
    this.search$.next('');
  }

  onSortChange(value: string): void {
    this.sortBy.set(value);
    this.currentPage.set(1);
    this.loadProducts();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;

    this.currentPage.set(page);
    this.loadProducts();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  private loadSeller(): void {
    this.sellerLoaded.set(false);
    this.seller.set(null);

    this.sellerService.getById(this.sellerId).subscribe({
      next: (response) => {
        this.seller.set(response.data);
        this.sellerLoaded.set(true);
      },
      error: () => {
        this.notFound.set(true);
        this.sellerLoaded.set(true);
      }
    });
  }

  private loadProducts(): void {
    this.request?.unsubscribe(); // نلغي أي طلب قديم لسه شغال
    this.isLoading.set(true);

    this.request = this.productService.search({
      sellerId: this.sellerId,
      searchTerm: this.searchTerm().trim(),
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