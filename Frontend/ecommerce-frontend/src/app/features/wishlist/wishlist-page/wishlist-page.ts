import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, Heart, Star } from 'lucide-angular';
import { WishlistService } from '../../../core/services/wishlist';
import { CartService } from '../../../core/services/cart';
import { ToastService } from '../../../core/services/toast';

@Component({
  selector: 'app-wishlist-page',
  standalone: true,
  imports: [CommonModule, RouterLink, LucideAngularModule],
  templateUrl: './wishlist-page.html',
  styleUrl: './wishlist-page.css'
})
export class WishlistPage implements OnInit {
  readonly HeartIcon = Heart;
  readonly StarIcon = Star;

  addingToCartId = signal<number | null>(null);
  removingId = signal<number | null>(null);

  constructor(
    public wishlistService: WishlistService,
    private cartService: CartService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.wishlistService.loadWishlist();
  }

  addToCart(productId: number): void {
    this.addingToCartId.set(productId);

    this.cartService.addItem({ productId, quantity: 1 }).subscribe({
      next: () => {
        this.addingToCartId.set(null);
        this.toastService.show('تمت الإضافة للسلة بنجاح', 'success');
      },
      error: () => this.addingToCartId.set(null)
    });
  }

  remove(productId: number): void {
    this.removingId.set(productId);

    this.wishlistService.removeItem(productId).subscribe({
      next: () => {
        this.removingId.set(null);
        this.toastService.show('تمت الإزالة من المفضلة', 'success');
      },
      error: () => this.removingId.set(null)
    });
  }
}