import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { AddToCartRequest, Cart } from '../models/cart.model';
import { ToastService } from './toast';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private apiUrl = `${environment.apiUrl}/cart`;

  cart = signal<Cart | null>(null);

  // عدد العناصر، بيتحسب تلقائيًا من الـ cart نفسه
  itemsCount = computed(() => this.cart()?.items.length ?? 0);

  constructor(
    private http: HttpClient,
    private toastService: ToastService
  ) {}

  loadCart(): void {
    this.http.get<ApiResponse<Cart>>(this.apiUrl).subscribe({
      next: (response) => {
        this.cart.set(response.data);

        // الباك بيشيل المنتجات اللي اتخفّت من السلة، فنعرّف العميل بدل ما تختفي من غير سبب
        if (response.data.removedCount > 0) {
          this.toastService.show('بعض المنتجات لم تعد متاحة وتم حذفها من سلتك', 'info');
        }
      }
    });
  }

  addItem(data: AddToCartRequest) {
    return this.http.post<ApiResponse<null>>(`${this.apiUrl}/items`, data).pipe(
      tap(() => this.loadCart()) // بعد الإضافة، نعيد جلب الكارت عشان يبقى محدّث
    );
  }

  updateItem(productId: number, quantity: number) {
    return this.http.put<ApiResponse<null>>(`${this.apiUrl}/items/${productId}`, { quantity }).pipe(
      tap(() => this.loadCart())
    );
  }

  removeItem(productId: number) {
    return this.http.delete<ApiResponse<null>>(`${this.apiUrl}/items/${productId}`).pipe(
      tap(() => this.loadCart())
    );
  }
}