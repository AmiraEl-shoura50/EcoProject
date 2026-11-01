import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { LucideAngularModule, Percent } from 'lucide-angular';
import { ProductService } from '../../../core/services/product';
import { Product } from '../../../core/models/product.model';
import { ProductCard } from '../../../shared/components/product-card/product-card';

@Component({
  selector: 'app-offers-page',
  standalone: true,
  imports: [CommonModule, RouterLink, LucideAngularModule, ProductCard],
  templateUrl: './offers-page.html',
  styleUrl: './offers-page.css'
})
export class OffersPage implements OnInit {
  readonly PercentIcon = Percent;

  products = signal<Product[]>([]);
  isLoading = signal<boolean>(true);
  sortBy = signal<string>('discount');
  totalCount = signal<number>(0);
  skeletonItems = [1, 2, 3, 4, 5, 6, 7, 8];

  currentPage = signal<number>(1);
  totalPages = signal<number>(1);
  hasPreviousPage = signal<boolean>(false);
  hasNextPage = signal<boolean>(false);

  sortOptions = [
    { value: 'discount', label: 'الأعلى خصمًا' },
    { value: 'newest', label: 'الأحدث' },
    { value: 'price_asc', label: 'السعر: من الأقل للأعلى' },
    { value: 'price_desc', label: 'السعر: من الأعلى للأقل' },
    { value: 'rating', label: 'الأعلى تقييمًا' }
  ];

  private readonly pageSize = 12;
  private request?: Subscription;

  constructor(private productService: ProductService) {}

  ngOnInit(): void {
    this.load();
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
    this.request?.unsubscribe();
    this.isLoading.set(true);

    this.request = this.productService.getOffers({
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

        // العدد المعروض في الـ Navbar يفضل متزامن مع الحقيقة
        this.productService.offersCount.set(result.totalCount);
      },
      error: () => this.isLoading.set(false)
    });
  }
}