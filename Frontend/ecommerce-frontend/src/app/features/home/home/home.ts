import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, Star } from 'lucide-angular';
import { ProductService } from '../../../core/services/product';
import { CategoryService } from '../../../core/services/category';
import { ReviewService } from '../../../core/services/review';
import { Product } from '../../../core/models/product.model';
import { Category } from '../../../core/models/category.model';
import { LatestReview } from '../../../core/models/review.model';
import { ProductCard } from '../../../shared/components/product-card/product-card';

interface StaticTestimonial {
  customerName: string;
  comment: string;
  rating: number;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, LucideAngularModule, ProductCard],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home implements OnInit {
  products = signal<Product[]>([]);
  categories = signal<Category[]>([]);
  latestReviews = signal<LatestReview[]>([]);
  isLoading = signal<boolean>(true);
  skeletonItems = [1, 2, 3, 4, 5, 6, 7, 8];
  stars = [1, 2, 3, 4, 5];

  readonly StarIcon = Star;

  // ✅ آراء ثابتة (تسويقية)
  staticTestimonials: StaticTestimonial[] = [
    {
      customerName: 'سارة أحمد',
      comment: 'تجربة تسوق ممتازة، المنتجات وصلت بسرعة والجودة فعلاً زي ما متوقعة.',
      rating: 5
    },
    {
      customerName: 'محمد علي',
      comment: 'أسعار منافسة وخدمة عملاء محترمة جدًا، هكرر التجربة أكيد.',
      rating: 5
    },
    {
      customerName: 'نور حسن',
      comment: 'الموقع سهل الاستخدام وفيه تنوع كبير في المنتجات.',
      rating: 4
    }
    
  ];

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private reviewService: ReviewService
  ) {}

  ngOnInit(): void {
    this.loadProducts();
    this.loadCategories();
    this.loadReviews();
  }

  private loadCategories(): void {
    this.categoryService.getTree().subscribe({
      next: (response) => this.categories.set(response.data)
    });
  }

  private loadReviews(): void {
    this.reviewService.getLatest(6).subscribe({
      next: (response) => this.latestReviews.set(response.data)
    });
  }

  private loadProducts(): void {
    this.isLoading.set(true);

    this.productService.search({ pageNumber: 1, pageSize: 12 }).subscribe({
      next: (response) => {
        this.products.set(response.data.items);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }
}