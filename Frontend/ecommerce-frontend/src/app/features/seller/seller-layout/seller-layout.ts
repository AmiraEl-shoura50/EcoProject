import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, LayoutDashboard, ClipboardList, Package, Layers, Percent, Store, Wallet } from 'lucide-angular';

@Component({
  selector: 'app-seller-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, LucideAngularModule],
  templateUrl: './seller-layout.html',
  styleUrl: './seller-layout.css'
})
export class SellerLayout {
  links = [
    { path: '/seller', label: 'نظرة عامة', icon: LayoutDashboard, exact: true },
    { path: '/seller/orders', label: 'الطلبات', icon: ClipboardList, exact: false },
    { path: '/seller/products', label: 'المنتجات', icon: Package, exact: false },
    { path: '/seller/categories', label: 'الأقسام الفرعية', icon: Layers, exact: false },
    { path: '/seller/discounts', label: 'الخصومات', icon: Percent, exact: false },
   { path: '/seller/earnings', label: 'المستحقات', icon: Wallet, exact: false },
    { path: '/seller/store', label: 'متجري', icon: Store, exact: false },
    
  ];
}