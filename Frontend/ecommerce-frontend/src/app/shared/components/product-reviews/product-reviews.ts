import { Component, effect, input, signal, untracked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReviewService } from '../../../core/services/review';
import { Review, ReviewSummary } from '../../../core/models/review.model';
import { Stars } from '../stars/stars';
import { TimeAgoPipe } from '../../pipes/time-ago.pipe';

@Component({
  selector: 'app-product-reviews',
  standalone: true,
  imports: [CommonModule, Stars, TimeAgoPipe],
  templateUrl: './product-reviews.html',
  styleUrl: './product-reviews.css'
})
export class ProductReviews {
  productId = input.required<number>();

  summary = signal<ReviewSummary | null>(null);
  reviews = signal<Review[]>([]);
  isLoading = signal<boolean>(true);
  isLoadingMore = signal<boolean>(false);
  hasNextPage = signal<boolean>(false);

  readonly starRows = [5, 4, 3, 2, 1];

  private readonly pageSize = 5;
  private page = 1;

  constructor(private reviewService: ReviewService) {
    // لما المنتج يتغيّر (التنقل بين المنتجات من غير ما الصفحة تتهدم) نعيد تحميل التقييمات
    effect(() => {
      const id = this.productId();
      untracked(() => this.load(id));
    });
  }

  barWidth(star: number): number {
    const summary = this.summary();
    if (!summary || summary.totalCount === 0) return 0;

    return (summary.distribution[star - 1] / summary.totalCount) * 100;
  }

  // أول اسم + أول حرف من الاسم التاني (خصوصية المقيّم)
  displayName(fullName: string): string {
    const parts = fullName.trim().split(/\s+/).filter(Boolean);
    if (parts.length === 0) return 'عميل';

    return parts.length > 1 ? `${parts[0]} ${parts[1].charAt(0)}.` : parts[0];
  }

  initial(fullName: string): string {
    return fullName.trim().charAt(0) || '؟';
  }

  loadMore(): void {
    const id = this.productId();

    this.isLoadingMore.set(true);
    this.page++;

    this.reviewService.getByProduct(id, this.page, this.pageSize).subscribe({
      next: (response) => {
        if (this.productId() !== id) return;

        this.reviews.update(list => [...list, ...response.data.items]);
        this.hasNextPage.set(response.data.hasNextPage);
        this.isLoadingMore.set(false);
      },
      error: () => {
        this.page--;
        this.isLoadingMore.set(false);
      }
    });
  }

  private load(productId: number): void {
    this.isLoading.set(true);
    this.page = 1;
    this.summary.set(null);
    this.reviews.set([]);
    this.hasNextPage.set(false);

    this.reviewService.getSummary(productId).subscribe({
      next: (response) => {
        if (this.productId() === productId) this.summary.set(response.data);
      }
    });

    this.reviewService.getByProduct(productId, 1, this.pageSize).subscribe({
      next: (response) => {
        if (this.productId() !== productId) return;

        this.reviews.set(response.data.items);
        this.hasNextPage.set(response.data.hasNextPage);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}