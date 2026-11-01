import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, ClipboardList, Package, Layers, Percent } from 'lucide-angular';

@Component({
  selector: 'app-seller-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, LucideAngularModule],
  templateUrl: './seller-layout.html',
  styleUrl: './seller-layout.css'
})
export class SellerLayout {


links = [
  { path: '/seller/orders', label: 'الطلبات', icon: ClipboardList },
  { path: '/seller/products', label: 'المنتجات', icon: Package },
  { path: '/seller/categories', label: 'الأقسام الفرعية', icon: Layers },
  { path: '/seller/discounts', label: 'الخصومات', icon: Percent }
];
}