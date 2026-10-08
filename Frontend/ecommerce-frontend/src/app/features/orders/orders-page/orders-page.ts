import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../../core/services/order';
import { ToastService } from '../../../core/services/toast';
import { Order } from '../../../core/models/order.model';

@Component({
  selector: 'app-orders-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './orders-page.html',
  styleUrl: './orders-page.css'
})
export class OrdersPage implements OnInit {
  orders = signal<Order[]>([]);
  isLoading = signal<boolean>(true);
  cancellingId = signal<number | null>(null);

  currentPage = signal<number>(1);
  totalPages = signal<number>(1);
  hasPreviousPage = signal<boolean>(false);
  hasNextPage = signal<boolean>(false);

  private readonly pageSize = 5;

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
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  statusLabel(status: string): string {
    return this.statusLabels[status] ?? status;
  }

  statusClass(status: string): string {
    return this.statusClasses[status] ?? 'bg-gray-100 text-gray-600';
  }

  canCancel(order: Order): boolean {
    return ['Pending', 'AwaitingConfirmation', 'Confirmed'].includes(order.status);
  }

  canUploadProof(order: Order): boolean {
    return order.status === 'Pending' && order.paymentMethodType === 'Manual';
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.currentPage.set(page);
    this.loadOrders();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  cancelOrder(order: Order): void {
    if (!confirm(`متأكدة إنك عايزة تلغي الطلب رقم ${order.id}؟`)) return;

    this.cancellingId.set(order.id);

    this.orderService.cancel(order.id).subscribe({
      next: () => {
        this.cancellingId.set(null);
        this.toastService.show('تم إلغاء الطلب بنجاح', 'success');
        this.loadOrders();
      },
      error: () => this.cancellingId.set(null)
    });
  }

  private loadOrders(): void {
    this.isLoading.set(true);

    this.orderService.getMyOrders(this.currentPage(), this.pageSize).subscribe({
      next: (response) => {
        const result = response.data;
        this.orders.set(result.items);
        this.totalPages.set(result.totalPages);
        this.hasPreviousPage.set(result.hasPreviousPage);
        this.hasNextPage.set(result.hasNextPage);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}