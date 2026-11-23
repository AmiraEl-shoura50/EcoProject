import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, LayoutDashboard, Users, Package, Layers, Wallet, CreditCard, Mail } from 'lucide-angular';
import { AdminMessageService } from '../../../core/services/admin-message';
import { AdminPaymentService } from '../../../core/services/admin-payment';

type BadgeKind = 'messages' | 'payments' | null;

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, LucideAngularModule],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css'
})
export class AdminLayout implements OnInit {
  // من غير نوع صريح: TypeScript بيستنتج نوع الأيقونة بنفسه، وبنثبّت بس نوع الـ badge
  links = [
    { path: '/admin', label: 'نظرة عامة', icon: LayoutDashboard, exact: true, badge: null as BadgeKind },
    { path: '/admin/payments', label: 'الدفعات', icon: Wallet, exact: false, badge: 'payments' as BadgeKind },
    { path: '/admin/users', label: 'المستخدمون', icon: Users, exact: false, badge: null as BadgeKind },
    { path: '/admin/products', label: 'المنتجات', icon: Package, exact: false, badge: null as BadgeKind },
    { path: '/admin/categories', label: 'الأقسام الرئيسية', icon: Layers, exact: false, badge: null as BadgeKind },
    { path: '/admin/payment-methods', label: 'طرق الدفع', icon: CreditCard, exact: false, badge: null as BadgeKind },
    { path: '/admin/messages', label: 'الرسائل', icon: Mail, exact: false, badge: 'messages' as BadgeKind }
  ];

  constructor(
    private messageService: AdminMessageService,
    private paymentService: AdminPaymentService
  ) {}

  ngOnInit(): void {
    this.messageService.refreshUnread();
    this.paymentService.refreshAwaiting();
  }

  badgeCount(kind: BadgeKind): number {
    if (kind === 'messages') return this.messageService.unreadCount();
    if (kind === 'payments') return this.paymentService.awaitingCount();
    return 0;
  }
}