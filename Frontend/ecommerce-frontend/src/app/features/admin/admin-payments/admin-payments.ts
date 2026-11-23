import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, Subscription, debounceTime, distinctUntilChanged, map } from 'rxjs';
import { LucideAngularModule, Search, X } from 'lucide-angular';
import { AdminPaymentService } from '../../../core/services/admin-payment';
import { ToastService } from '../../../core/services/toast';
import { AdminPaymentGroup } from '../../../core/models/payment.model';

type PaymentFilter = 'awaiting' | 'pending' | 'paid' | 'all';

@Component({
  selector: 'app-admin-payments',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, LucideAngularModule],
  templateUrl: './admin-payments.html',
  styleUrl: './admin-payments.css'
})
export class AdminPayments implements OnInit {
  readonly SearchIcon = Search;
  readonly CloseIcon = X;

  readonly filters: { key: PaymentFilter; label: string }[] = [
    { key: 'awaiting', label: 'في انتظار المراجعة' },
    { key: 'pending', label: 'في انتظار الدفع' },
    { key: 'paid', label: 'مدفوعة' },
    { key: 'all', label: 'الكل' }
  ];

  items = signal<AdminPaymentGroup[]>([]);
  isLoading = signal<boolean>(true);
  totalCount = signal<number>(0);

  filter = signal<PaymentFilter>('awaiting');
  searchTerm = signal<string>('');

  rejectingId = signal<number | null>(null);
  rejectionReason = '';
  busyId = signal<number | null>(null);
  previewImage = signal<string | null>(null);

  currentPage = signal<number>(1);
  totalPages = signal<number>(1);
  hasPreviousPage = signal<boolean>(false);
  hasNextPage = signal<boolean>(false);

  private readonly pageSize = 10;
  private search$ = new Subject<string>();
  private request?: Subscription;
  private destroyRef = inject(DestroyRef);

  constructor(
    public paymentService: AdminPaymentService,
    private toastService: ToastService
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
        this.load();
      });

    this.load();
    this.paymentService.refreshAwaiting();
  }

  setFilter(filter: PaymentFilter): void {
    if (this.filter() === filter) return;

    this.filter.set(filter);
    this.currentPage.set(1);
    this.rejectingId.set(null);
    this.load();
  }

  onSearchInput(value: string): void {
    this.searchTerm.set(value);
    this.search$.next(value);
  }

  clearSearch(): void {
    this.searchTerm.set('');
    this.search$.next('');
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;

    this.currentPage.set(page);
    this.load();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  // لو في تلاعب: المجموع في جدول البائعين لازم يطابق المبلغ الكلي
  canReview(group: AdminPaymentGroup): boolean {
    return group.status === 'AwaitingConfirmation';
  }

  approve(group: AdminPaymentGroup): void {
    const question = `هل استلمت فعلًا ${group.totalAmount} جنيه؟ هيتم تأكيد ${this.activeOrdersCount(group)} طلب وإشعار البائعين.`;
    if (!confirm(question)) return;

    this.busyId.set(group.id);

    this.paymentService.review(group.id, true).subscribe({
      next: () => {
        this.busyId.set(null);
        this.toastService.show('تم تأكيد الدفع وإشعار البائعين', 'success');
        this.load(true);
      },
      error: () => this.busyId.set(null)
    });
  }

  startReject(group: AdminPaymentGroup): void {
    this.rejectingId.set(group.id);
    this.rejectionReason = '';
  }

  cancelReject(): void {
    this.rejectingId.set(null);
  }

  confirmReject(group: AdminPaymentGroup): void {
    const reason = this.rejectionReason.trim();
    if (!reason) {
      this.toastService.show('اكتب سبب الرفض ليظهر للعميل', 'error');
      return;
    }

    this.busyId.set(group.id);

    this.paymentService.review(group.id, false, reason).subscribe({
      next: () => {
        this.busyId.set(null);
        this.rejectingId.set(null);
        this.toastService.show('تم رفض الإثبات وإشعار العميل', 'success');
        this.load(true);
      },
      error: () => this.busyId.set(null)
    });
  }

  statusLabel(status: string): string {
    switch (status) {
      case 'AwaitingConfirmation': return 'في انتظار المراجعة';
      case 'Pending': return 'في انتظار الدفع';
      case 'Paid': return 'مدفوعة';
      default: return 'ملغية';
    }
  }

  statusClass(status: string): string {
    switch (status) {
      case 'AwaitingConfirmation': return 'bg-blue-100 text-blue-700';
      case 'Pending': return 'bg-yellow-100 text-yellow-700';
      case 'Paid': return 'bg-green-100 text-green-700';
      default: return 'bg-gray-200 text-gray-600';
    }
  }

  activeOrdersCount(group: AdminPaymentGroup): number {
    return group.orders.filter(o => o.status !== 'Cancelled').length;
  }

  private load(silent = false): void {
    this.request?.unsubscribe(); // نلغي أي طلب قديم لسه شغال
    if (!silent) this.isLoading.set(true);

    this.request = this.paymentService.getAll({
      filter: this.filter(),
      search: this.searchTerm().trim(),
      pageNumber: this.currentPage(),
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        const result = response.data;

        // لو الصفحة فضيت وفي صفحات قبلها، نرجع صفحة لورا
        if (result.items.length === 0 && this.currentPage() > 1) {
          this.currentPage.set(this.currentPage() - 1);
          this.load(silent);
          return;
        }

        this.items.set(result.items);
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