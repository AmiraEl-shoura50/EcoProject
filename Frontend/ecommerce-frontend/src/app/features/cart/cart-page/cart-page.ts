import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, Minus, Plus, Trash2 } from 'lucide-angular';
import { CartService } from '../../../core/services/cart';
import { ToastService } from '../../../core/services/toast';

@Component({
  selector: 'app-cart-page',
  standalone: true,
  imports: [CommonModule, RouterLink, LucideAngularModule],
  templateUrl: './cart-page.html',
  styleUrl: './cart-page.css'
})
export class CartPage implements OnInit {
  readonly MinusIcon = Minus;
  readonly PlusIcon = Plus;
  readonly TrashIcon = Trash2;

  constructor(
    public cartService: CartService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.cartService.loadCart();
  }

  increaseQuantity(productId: number, currentQuantity: number): void {
    this.cartService.updateItem(productId, currentQuantity + 1).subscribe();
  }

  decreaseQuantity(productId: number, currentQuantity: number): void {
    if (currentQuantity <= 1) return;
    this.cartService.updateItem(productId, currentQuantity - 1).subscribe();
  }

  removeItem(productId: number): void {
    this.cartService.removeItem(productId).subscribe({
      next: () => this.toastService.show('تم حذف المنتج من السلة', 'success')
    });
  }
}