import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, Users, Layers, CreditCard } from 'lucide-angular';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, LucideAngularModule],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css'
})
export class AdminLayout {
  links = [
    { path: '/admin/users', label: 'المستخدمون', icon: Users },
    { path: '/admin/categories', label: 'الأقسام الرئيسية', icon: Layers },
    { path: '/admin/payment-methods', label: 'طرق الدفع', icon: CreditCard }
  ];
}