import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DashboardService } from '../../../core/services/dashboard';
import { SellerDashboardData } from '../../../core/models/dashboard.model';
import { SalesChart } from '../../../shared/components/sales-chart/sales-chart';
import { Stars } from '../../../shared/components/stars/stars';

type Tone = 'blue' | 'green' | 'indigo' | 'orange';

interface AttentionCard {
  label: string;
  hint: string;
  value: number;
  tone: Tone;
  urgent: boolean;
  link: string;
  query: Record<string, string> | null;
}

@Component({
  selector: 'app-seller-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, SalesChart, Stars],
  templateUrl: './seller-dashboard.html',
  styleUrl: './seller-dashboard.css'
})
export class SellerDashboardPage implements OnInit {
  data = signal<SellerDashboardData | null>(null);
  isLoading = signal<boolean>(true);
  failed = signal<boolean>(false);

  private readonly toneClasses: Record<Tone, string> = {
    blue: 'bg-blue-50 text-blue-700',
    green: 'bg-green-50 text-green-700',
    indigo: 'bg-indigo-50 text-indigo-700',
    orange: 'bg-orange-50 text-orange-700'
  };

  // البطاقات اللي بتحتاج قرار من البائع، وكل واحدة بتفتح الصفحة المناسبة بالفلتر الجاهز
  cards = computed<AttentionCard[]>(() => {
    const d = this.data();
    if (!d) return [];

    const stockIssues = d.lowStockCount + d.outOfStockCount;

    return [
      {
        label: 'تحتاج مراجعة',
        hint: d.ordersToReview > 0 ? 'إثبات دفع ينتظر قرارك' : 'لا يوجد ما ينتظر المراجعة',
        value: d.ordersToReview,
        tone: 'blue',
        urgent: d.ordersToReview > 0,
        link: '/seller/orders',
        query: { filter: 'review' }
      },
      {
        label: 'جاهزة للشحن',
        hint: d.ordersToShip > 0 ? 'دفعها اتأكد وتنتظر الشحن' : 'لا توجد طلبات للشحن',
        value: d.ordersToShip,
        tone: 'green',
        urgent: d.ordersToShip > 0,
        link: '/seller/orders',
        query: { filter: 'Confirmed' }
      },
      {
        label: 'في الطريق',
        hint: d.ordersInTransit > 0 ? 'بانتظار تأكيد التوصيل' : 'لا توجد طلبات في الطريق',
        value: d.ordersInTransit,
        tone: 'indigo',
        urgent: false,
        link: '/seller/orders',
        query: { filter: 'Shipped' }
      },
      {
        label: 'مخزون يحتاج انتباه',
        hint: stockIssues > 0
          ? `${d.outOfStockCount} نفذت و${d.lowStockCount} قاربت على النفاد`
          : 'مخزونك بخير',
        value: stockIssues,
        tone: 'orange',
        urgent: stockIssues > 0,
        link: '/seller/products',
        query: { stock: 'attention' }
      }
    ];
  });

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.load();
  }

  toneClass(tone: Tone): string {
    return this.toneClasses[tone];
  }

  load(): void {
    this.isLoading.set(true);
    this.failed.set(false);

    this.dashboardService.getSeller().subscribe({
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