import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { CheckoutResult, CreateOrderRequest, Order, PaymentProof } from '../models/order.model';
import { CartService } from './cart';
import { PaginatedResult } from '../models/product.model';


@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private apiUrl = `${environment.apiUrl}/orders`;

  constructor(
    private http: HttpClient,
    private cartService: CartService
  ) {}

  checkout(data: CreateOrderRequest): Observable<ApiResponse<CheckoutResult[]>> {
    return this.http.post<ApiResponse<CheckoutResult[]>>(`${this.apiUrl}/checkout`, data).pipe(
      tap(() => this.cartService.loadCart()) // ✅ الكارت بيتفضّى بعد الطلب، نحدّثه
    );
  }
 submitPaymentProof(orderId: number, image: File, transferReference?: string): Observable<ApiResponse<PaymentProof>> {
  const formData = new FormData();
  formData.append('Image', image);
  if (transferReference) {
    formData.append('TransferReference', transferReference);
  }

  return this.http.post<ApiResponse<PaymentProof>>(`${this.apiUrl}/${orderId}/payment-proof`, formData);
}
getMyOrders(pageNumber: number, pageSize: number): Observable<ApiResponse<PaginatedResult<Order>>> {
  return this.http.get<ApiResponse<PaginatedResult<Order>>>(this.apiUrl, {
    params: { pageNumber, pageSize }
  });
}

cancel(orderId: number): Observable<unknown> {
  return this.http.put(`${this.apiUrl}/${orderId}/cancel`, {});
}
}