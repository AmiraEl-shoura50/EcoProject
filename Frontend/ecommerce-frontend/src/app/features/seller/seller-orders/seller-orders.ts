import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { LucideAngularModule, X } from 'lucide-angular';
import { OrderService } from '../../../core/services/order';
import { ToastService } from '../../../core/services/toast';
import { SellerOrder } from '../../../core/models/order.model';
import { RouterLink, ActivatedRoute } from '@angular/router'; 

@Component({
  selector: 'app-seller-orders',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule, RouterLink],
  templateUrl: './seller-orders.html',
  styleUrl: './seller-orders.css'
})
export class SellerOrders implements OnInit {
  readonly CloseIcon = X;

  orders = signal<SellerOrder[]>([]);
  isLoading = signal<boolean>(true);
  filter = signal<string>('all');
  busyId = signal<number | null>(null);
  previewImage = signal<string | null>(null);
  rejectingId = signal<number | null>(null);
  rejectionReason = '';

  // ---- التحديد والحذف ----
  selectedIds = signal<number[]>([]);
  isDeleting = signal<boolean>(false);

  filters = [
    { key: 'all', label: 'الكل' },
    { key: 'review', label: 'تحتاج مراجعة' },
    { key: 'Confirmed', label: 'مؤكدة' },
    { key: 'Shipped', label: 'تم الشحن' },
    { key: 'Delivered', label: 'تم التوصيل' },
    { key: 'Cancelled', label: 'ملغية' }
  ];

  filteredOrders = computed(() =>
    this.orders().filter(o => this.matches(o, this.filter()))
  );

  // الطلبات الملغية الظاهرة حاليًا (هي الوحيدة القابلة للحذف)
  visibleCancelled = computed(() =>
    this.filteredOrders().filter(o => o.status === 'Cancelled')
  );

  allCancelledSelected = computed(() =>
    this.visibleCancelled().length > 0 &&
    this.visibleCancelled().every(o => this.selectedIds().includes(o.id))
  );

  private readonly statusLabels: Record<string, string> = {
    Pending: 'في انتظار الدفع',
    AwaitingConfirmation: 'في انتظار تأكيد الدفع',
    Confirmed: 'مؤكد',
    Shipped: 'تم الشحن',
    Delivered: 'تم التوصيل',
    Cancelled: 'ملغي'
  };

  private readonly statusClasses: Record<string, string> = {
    Pending: 'bg-yellow-100 text-yellow-700',
    AwaitingConfirmation: 'bg-blue-100 text-blue-700',
    Confirmed: 'bg-green-100 text-green-700',
    Shipped: 'bg-indigo-100 text-indigo-700',
    Delivered: 'bg-emerald-100 text-emerald-700',
    Cancelled: 'bg-red-100 text-red-600'
  };

  constructor(
    private orderService: OrderService,
    private toastService: ToastService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
  this.loadOrders();

  // لو جاي من بطاقة في النظرة العامة (?filter=review) نفتح على الفلتر ده
  this.route.queryParamMap.subscribe(params => {
    const key = params.get('filter');

    if (key && this.filters.some(f => f.key === key)) {
      this.filter.set(key);
    }
  });
}
  statusLabel(status: string): string {
    return this.statusLabels[status] ?? status;
  }

  statusClass(status: string): string {
    return this.statusClasses[status] ?? 'bg-gray-100 text-gray-600';
  }

    needsReview(order: SellerOrder): boolean {
    return false; // مراجعة الدفع بقت للأدمن
  }

  countFor(key: string): number {
    return this.orders().filter(o => this.matches(o, key)).length;
  }

  setFilter(key: string): void {
    this.filter.set(key);
    this.selectedIds.set([]); // التحديد مبيفضلش على طلبات مش ظاهرة
  }

  // ---- التحديد والحذف ----
  isSelected(orderId: number): boolean {
    return this.selectedIds().includes(orderId);
  }

  toggleSelect(orderId: number): void {
    this.selectedIds.update(ids =>
      ids.includes(orderId) ? ids.filter(id => id !== orderId) : [...ids, orderId]
    );
  }

  toggleSelectAll(): void {
    this.selectedIds.set(this.allCancelledSelected() ? [] : this.visibleCancelled().map(o => o.id));
  }

  deleteSelected(): void {
    const ids = this.selectedIds();
    if (ids.length === 0) return;

    if (!confirm(`هل تريد حذف ${ids.length} طلب ملغي من قائمتك؟`)) return;

    this.isDeleting.set(true);

    this.orderService.deleteCancelledAsSeller(ids).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.selectedIds.set([]);
        this.toastService.show('تم حذف الطلبات المحددة', 'success');
        this.loadOrders();
      },
      error: () => this.isDeleting.set(false)
    });
  }

  // ---- الإجراءات ----
  approve(order: SellerOrder): void {
    if (!confirm(`هل تأكدت من وصول المبلغ وتريد قبول الدفع للطلب #${order.id}؟`)) return;

    this.run(order.id, this.orderService.reviewPayment(order.id, true), 'تم قبول الدفع وتأكيد الطلب');
  }

  startReject(order: SellerOrder): void {
    this.rejectionReason = '';
    this.rejectingId.set(order.id);
  }

  cancelReject(): void {
    this.rejectingId.set(null);
  }

  confirmReject(order: SellerOrder): void {
    const reason = this.rejectionReason.trim();
    if (!reason) {
      this.toastService.show('اكتب سبب الرفض ليظهر للعميل', 'error');
      return;
    }

    this.run(
      order.id,
      this.orderService.reviewPayment(order.id, false, reason),
      'تم رفض إثبات الدفع',
      () => this.rejectingId.set(null)
    );
  }

  advance(order: SellerOrder, newStatus: 'Shipped' | 'Delivered'): void {
    const question = newStatus === 'Shipped'
      ? `هل تريد تأكيد شحن الطلب #${order.id}؟`
      : `هل تريد تأكيد توصيل الطلب #${order.id}؟`;
    if (!confirm(question)) return;

    const message = newStatus === 'Shipped' ? 'تم تحديث الطلب: تم الشحن' : 'تم تحديث الطلب: تم التوصيل';
    this.run(order.id, this.orderService.updateStatus(order.id, newStatus), message);
  }

  cancel(order: SellerOrder): void {
    if (!confirm(`هل تريد إلغاء الطلب #${order.id}؟ سيتم إرجاع الكميات للمخزون.`)) return;

    this.run(order.id, this.orderService.updateStatus(order.id, 'Cancelled'), 'تم إلغاء الطلب وإرجاع الكميات للمخزون');
  }

  private matches(order: SellerOrder, key: string): boolean {
    if (key === 'all') return true;
    if (key === 'review') return this.needsReview(order);
    return order.status === key;
  }

  // تنفيذ أي إجراء بنفس الخطوات: تعطيل الأزرار، رسالة نجاح، إعادة تحميل
  private run(orderId: number, request: Observable<unknown>, successMessage: string, onDone?: () => void): void {
    this.busyId.set(orderId);

    request.subscribe({
      next: () => {
        this.busyId.set(null);
        onDone?.();
        this.toastService.show(successMessage, 'success');
        this.loadOrders();
      },
      error: () => this.busyId.set(null)
    });
  }

  private loadOrders(): void {
    this.orderService.getSellerOrders().subscribe({
      next: (response) => {
        this.orders.set(response.data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}