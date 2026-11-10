import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DashboardService } from '../../../core/services/dashboard';
import { AdminDashboardData } from '../../../core/models/dashboard.model';
import { SalesChart } from '../../../shared/components/sales-chart/sales-chart';

interface StatCard {
  label: string;
  value: number;
  hint: string;
  link: string | null;
  query: Record<string, string> | null;
}

interface StatusRow {
  key: string;
  label: string;
  count: number;
  percent: number;
  barClass: string;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, SalesChart],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboardPage implements OnInit {
  data = signal<AdminDashboardData | null>(null);
  isLoading = signal<boolean>(true);
  failed = signal<boolean>(false);

  private readonly statusOrder = ['Pending', 'AwaitingConfirmation', 'Confirmed', 'Shipped', 'Delivered', 'Cancelled'];

  private readonly statusLabels: Record<string, string> = {
    Pending: 'في انتظار الدفع',
    AwaitingConfirmation: 'في انتظار تأكيد الدفع',
    Confirmed: 'مؤكد',
    Shipped: 'تم الشحن',
    Delivered: 'تم التوصيل',
    Cancelled: 'ملغي'
  };

  private readonly statusBars: Record<string, string> = {
    Pending: 'bg-yellow-400',
    AwaitingConfirmation: 'bg-blue-400',
    Confirmed: 'bg-green-500',
    Shipped: 'bg-indigo-500',
    Delivered: 'bg-emerald-500',
    Cancelled: 'bg-red-400'
  };

  cards = computed<StatCard[]>(() => {
    const d = this.data();
    if (!d) return [];

    return [
      { label: 'المستخدمون', value: d.totalUsers, hint: 'إجمالي الحسابات', link: '/admin/users', query: null },
      { label: 'العملاء', value: d.customersCount, hint: 'حسابات المشترين', link: '/admin/users', query: { role: 'Customer' } },
      { label: 'البائعون', value: d.sellersCount, hint: 'حسابات البائعين', link: '/admin/users', query: { role: 'Seller' } },
      {
        label: 'حسابات موقوفة',
        value: d.suspendedUsers,
        hint: d.suspendedUsers > 0 ? 'لا يمكنها تسجيل الدخول' : 'لا توجد حسابات موقوفة',
        link: null,
        query: null
      },
      { label: 'المنتجات', value: d.productsCount, hint: 'على كل المتاجر', link: null, query: null },
      { label: 'الأقسام', value: d.categoriesCount, hint: 'رئيسية وفرعية', link: '/admin/categories', query: null }
    ];
  });

  statusRows = computed<StatusRow[]>(() => {
    const d = this.data();
    if (!d) return [];

    const total = Object.values(d.ordersByStatus).reduce((sum, count) => sum + count, 0);

    return this.statusOrder.map(key => {
      const count = d.ordersByStatus[key] ?? 0;

      return {
        key,
        label: this.statusLabels[key],
        count,
        percent: total > 0 ? (count / total) * 100 : 0,
        barClass: this.statusBars[key]
      };
    });
  });

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.failed.set(false);

    this.dashboardService.getAdmin().subscribe({
      next: (response) => {
        this.data.set(response.data);
        this.isLoading.set(false);
      },
      error: () => {
        this.failed.set(true);
        this.isLoading.set(false);
      }
    });
  }
}