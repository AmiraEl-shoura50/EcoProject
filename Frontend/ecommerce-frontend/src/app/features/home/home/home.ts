import { Component, OnInit, signal ,OnDestroy} from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, Star ,Percent } from 'lucide-angular';
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
export class Home implements OnInit , OnDestroy {
  products = signal<Product[]>([]);
  categories = signal<Category[]>([]);
  latestReviews = signal<LatestReview[]>([]);
  isLoading = signal<boolean>(true);
  skeletonItems = [1, 2, 3, 4, 5, 6, 7, 8];
  stars = [1, 2, 3, 4, 5];
  heroSlides = [
      {
        image: '/images/hero-product-1.png',
        alt: 'للأناقة عنوان '
      },
      {
        image: '/images/hero-product-2.png',
        alt: 'الاكثر مبيعاً'
      },
      {
        image: '/images/hero-product-3.png',
        alt: 'تسوق منتجات Smart Shop'
      }
    ];
     activeIndex = 0;

  private autoplayTimer?: ReturnType<typeof setInterval>;

  readonly StarIcon = Star;
  readonly PercentIcon = Percent;
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
    public productService: ProductService,
    private categoryService: CategoryService,
    private reviewService: ReviewService
  ) {}
 get previousIndex(): number {
    return (this.activeIndex - 1 + this.heroSlides.length) % this.heroSlides.length;
  }

  get nextIndex(): number {
    return (this.activeIndex + 1) % this.heroSlides.length;
  }
  ngOnInit(): void {
    this.loadProducts();
    this.loadCategories();
    this.loadReviews();
     this.startAutoplay();
  }
ngOnDestroy(): void {
    this.stopAutoplay();
  }
   nextSlide(): void {
    this.activeIndex = this.nextIndex;
    this.restartAutoplay();
  }

  previousSlide(): void {
    this.activeIndex = this.previousIndex;
    this.restartAutoplay();
  }

  goToSlide(index: number): void {
    this.activeIndex = index;
    this.restartAutoplay();
  }

  private startAutoplay(): void {
    this.autoplayTimer = setInterval(() => {
      this.activeIndex = this.nextIndex;
    }, 5000);
  }

  private stopAutoplay(): void {
    if (this.autoplayTimer) {
      clearInterval(this.autoplayTimer);
    }
  }

  private restartAutoplay(): void {
    this.stopAutoplay();
    this.startAutoplay();
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