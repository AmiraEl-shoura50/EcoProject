import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastContainer } from './shared/components/toast-container/toast-container';
import { Navbar } from './shared/components/navbar/navbar';
import { Footer } from './shared/components/footer/footer';
import { AuthService } from './core/services/auth';
import { CartService } from './core/services/cart';
import { WishlistService } from './core/services/wishlist';
import { ProductService } from './core/services/product';
import { PwaInstallService } from './core/services/pwa-install';
import { NotificationService } from './core/services/notification';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastContainer, Navbar, Footer],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  constructor(
    private authService: AuthService,
    private cartService: CartService,
    private wishlistService: WishlistService,
    private productService: ProductService,
    private pwaInstall: PwaInstallService , // بنعمله هنا عشان يسمع حدث التثبيت من أول لحظة
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
     this.registerShareWorker();
  this.productService.loadOffersCount();

  if (!this.authService.isLoggedIn()) return;

  this.notificationService.start(); // الإشعارات لكل الأدوار

  if (this.authService.userRole() === 'Customer') {
    this.cartService.loadCart();
    this.wishlistService.loadWishlist();
  }
  
  }

  private registerShareWorker(): void {
    if (!('serviceWorker' in navigator)) return;

    // لو فشل التسجيل الموقع بيكمل عادي، والمشاركة بس هي اللي مش هتشتغل
    navigator.serviceWorker.register('/share-sw.js').catch(() => {});
  }
}