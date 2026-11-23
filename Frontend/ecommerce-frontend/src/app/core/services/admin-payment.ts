import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { PaginatedResult } from '../models/product.model';
import { AdminPaymentGroup } from '../models/payment.model';

type CountResponse = ApiResponse<{ count: number }>;

@Injectable({
  providedIn: 'root'
})
export class AdminPaymentService {
  private apiUrl = `${environment.apiUrl}/admin/payments`;

  // عدد الدفعات المنتظرة مراجعة (بيظهر في القائمة الجانبية)
  awaitingCount = signal<number>(0);

  constructor(private http: HttpClient) {}

  getAll(params: {
    filter: string;
    search: string;
    pageNumber: number;
    pageSize: number;
  }): Observable<ApiResponse<PaginatedResult<AdminPaymentGroup>>> {
    return this.http.get<ApiResponse<PaginatedResult<AdminPaymentGroup>>>(this.apiUrl, { params });
  }

  refreshAwaiting(): void {
    this.http.get<CountResponse>(`${this.apiUrl}/awaiting-count`).subscribe({
      next: (response) => this.awaitingCount.set(response.data.count)
    });
  }

  review(id: number, approve: boolean, rejectionReason?: string): Observable<CountResponse> {
    return this.http.put<CountResponse>(`${this.apiUrl}/${id}/review`, { approve, rejectionReason }).pipe(
      tap((response) => this.awaitingCount.set(response.data.count))
    );
  }
}