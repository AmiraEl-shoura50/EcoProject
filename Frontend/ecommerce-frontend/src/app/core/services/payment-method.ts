import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import {
  AdminPaymentMethod,
  CreatePaymentMethodRequest,
  PaymentMethod,
  UpdatePaymentMethodRequest
} from '../models/payment-method.model';

@Injectable({
  providedIn: 'root'
})
export class PaymentMethodService {
  private apiUrl = `${environment.apiUrl}/paymentmethods`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<PaymentMethod[]>> {
    return this.http.get<ApiResponse<PaymentMethod[]>>(this.apiUrl);
  }

  // ---- خاص بالأدمن ----
  getAllForAdmin(): Observable<ApiResponse<AdminPaymentMethod[]>> {
    return this.http.get<ApiResponse<AdminPaymentMethod[]>>(`${this.apiUrl}/admin`);
  }

  create(data: CreatePaymentMethodRequest): Observable<ApiResponse<PaymentMethod>> {
    return this.http.post<ApiResponse<PaymentMethod>>(this.apiUrl, data);
  }

  update(id: number, data: UpdatePaymentMethodRequest): Observable<unknown> {
    return this.http.put(`${this.apiUrl}/${id}`, data);
  }

  delete(id: number): Observable<unknown> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
    setVisibility(id: number, isHidden: boolean): Observable<unknown> {
    return this.http.put(`${this.apiUrl}/${id}/visibility`, { isHidden });
  }
}