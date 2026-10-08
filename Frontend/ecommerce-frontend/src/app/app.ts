import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastContainer } from './shared/components/toast-container/toast-container';
import { Navbar } from './shared/components/navbar/navbar';
import { AuthService } from './core/services/auth';
import { CartService } from './core/services/cart';
import { WishlistService } from './core/services/wishlist';
import { Footer } from './shared/components/footer/footer';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastContainer, Navbar ,Footer],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  constructor(
    private authService: AuthService,
    private cartService: CartService,
    private wishlistService: WishlistService
  ) {
    
  }

  ngOnInit(): void {
    // ✅ لو المستخدم أصلاً مسجل دخول (توكن موجود من جلسة سابقة)، نجيب بياناته فورًا
    if (this.authService.isLoggedIn()) {
      this.cartService.loadCart();
      this.wishlistService.loadWishlist();
    }
  }
}