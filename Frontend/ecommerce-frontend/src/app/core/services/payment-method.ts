import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { PaymentMethod } from '../models/payment-method.model';

@Injectable({
  providedIn: 'root'
})
export class PaymentMethodService {
  private apiUrl = `${environment.apiUrl}/paymentmethods`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<PaymentMethod[]>> {
    return this.http.get<ApiResponse<PaymentMethod[]>>(this.apiUrl);
  }
}