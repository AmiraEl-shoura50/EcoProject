import { Component, HostListener, computed, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, Star, X } from 'lucide-angular';
import { ReviewService } from '../../../core/services/review';
import { ToastService } from '../../../core/services/toast';

@Component({
  selector: 'app-review-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './review-modal.html',
  styleUrl: './review-modal.css'
})
export class ReviewModal {
  readonly StarIcon = Star;
  readonly CloseIcon = X;
  readonly positions = [1, 2, 3, 4, 5];
  readonly maxLength = 1000;

  private readonly labels = ['', 'سيئ', 'مقبول', 'جيد', 'جيد جدًا', 'ممتاز'];

  orderId = input.required<number>();
  productId = input.required<number>();
  productName = input.required<string>();

  closed = output<void>();
  submitted = output<void>();

  rating = signal<number>(0);
  hovered = signal<number>(0);
  isSubmitting = signal<boolean>(false);
  comment = '';

  // النجوم بتتلون حسب الماوس لو فوقها، وإلا حسب اللي اختاره المستخدم
  activeRating = computed(() => this.hovered() || this.rating());

  constructor(
    private reviewService: ReviewService,
    private toastService: ToastService
  ) {}

  label(): string {
    return this.labels[this.activeRating()];
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (!this.isSubmitting()) this.closed.emit();
  }

  submit(): void {
    if (this.rating() < 1) {
      this.toastService.show('اختر عدد النجوم أولًا', 'error');
      return;
    }

    this.isSubmitting.set(true);

    this.reviewService.create({
      productId: this.productId(),
      orderId: this.orderId(),
      rating: this.rating(),
      comment: this.comment.trim() || null
    }).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.toastService.show('شكرًا لك، تم إضافة تقييمك', 'success');
        this.submitted.emit();
      },
      error: () => this.isSubmitting.set(false) // رسالة الخطأ بتظهر من الـ Interceptor
    });
  }
}