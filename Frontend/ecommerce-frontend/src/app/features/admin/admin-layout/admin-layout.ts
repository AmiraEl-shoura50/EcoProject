import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, LayoutDashboard, Users, Layers, CreditCard, Mail } from 'lucide-angular';
import { AdminMessageService } from '../../../core/services/admin-message';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, LucideAngularModule],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css'
})
export class AdminLayout implements OnInit {
  links = [
    { path: '/admin', label: 'نظرة عامة', icon: LayoutDashboard, exact: true, badge: false },
    { path: '/admin/users', label: 'المستخدمون', icon: Users, exact: false, badge: false },
    { path: '/admin/categories', label: 'الأقسام الرئيسية', icon: Layers, exact: false, badge: false },
    { path: '/admin/payment-methods', label: 'طرق الدفع', icon: CreditCard, exact: false, badge: false },
    { path: '/admin/messages', label: 'الرسائل', icon: Mail, exact: false, badge: true }
  ];

  constructor(public messageService: AdminMessageService) {}

  ngOnInit(): void {
    this.messageService.refreshUnread();
  }
}