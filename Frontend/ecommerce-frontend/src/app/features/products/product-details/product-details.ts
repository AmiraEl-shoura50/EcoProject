import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { LucideAngularModule, Heart, Minus, Plus, ShoppingCart } from 'lucide-angular';
import { ProductService } from '../../../core/services/product';
import { CartService } from '../../../core/services/cart';
import { WishlistService } from '../../../core/services/wishlist';
import { AuthService } from '../../../core/services/auth';
import { ToastService } from '../../../core/services/toast';
import { Product } from '../../../core/models/product.model';

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './product-details.html',
  styleUrl: './product-details.css'
})
export class ProductDetails implements OnInit {
  product = signal<Product | null>(null);
  isLoading = signal<boolean>(true);
  quantity = signal<number>(1);
  isAddingToCart = signal<boolean>(false);

  readonly HeartIcon = Heart;
  readonly MinusIcon = Minus;
  readonly PlusIcon = Plus;
  readonly CartIcon = ShoppingCart;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    public cartService: CartService,
    public wishlistService: WishlistService,
    public authService: AuthService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.loadProduct(+params['id']);
    });
  }

  private loadProduct(id: number): void {
    this.isLoading.set(true);
    this.quantity.set(1);

    this.productService.getById(id).subscribe({
      next: (response) => {
        this.product.set(response.data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  increaseQuantity(): void {
    const max = this.product()?.stockQuantity ?? 1;
    if (this.quantity() < max) {
      this.quantity.update(q => q + 1);
    }
  }

  decreaseQuantity(): void {
    if (this.quantity() > 1) {
      this.quantity.update(q => q - 1);
    }
  }

  addToCart(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login'], { queryParams: { returnUrl: this.router.url } });
      return;
    }

    const product = this.product();
    if (!product) return;

    this.isAddingToCart.set(true);

    this.cartService.addItem({ productId: product.id, quantity: this.quantity() }).subscribe({
      next: () => {
        this.isAddingToCart.set(false);
        this.toastService.show('تمت الإضافة للسلة بنجاح', 'success');
      },
      error: () => {
        this.isAddingToCart.set(false);
      }
    });
  }

  toggleWishlist(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login'], { queryParams: { returnUrl: this.router.url } });
      return;
    }

    const product = this.product();
    if (!product) return;

    if (this.wishlistService.isInWishlist(product.id)) {
      this.wishlistService.removeItem(product.id).subscribe({
        next: () => this.toastService.show('تمت الإزالة من المفضلة', 'success')
      });
    } else {
      this.wishlistService.addItem(product.id).subscribe({
        next: () => this.toastService.show('تمت الإضافة للمفضلة', 'success')
      });
    }
  }
}