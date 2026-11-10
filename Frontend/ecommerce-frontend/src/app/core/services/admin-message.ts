import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { ContactMessageItem } from '../models/contact-message.model';
import { PaginatedResult } from '../models/product.model';

type CountResponse = ApiResponse<{ count: number }>;

@Injectable({
  providedIn: 'root'
})
export class AdminMessageService {
  private apiUrl = `${environment.apiUrl}/admin/messages`;

  // عدد الرسائل غير المقروءة (بيظهر في القائمة الجانبية)
  unreadCount = signal<number>(0);

  constructor(private http: HttpClient) {}

  getAll(params: {
    filter: string;
    search: string;
    pageNumber: number;
    pageSize: number;
  }): Observable<ApiResponse<PaginatedResult<ContactMessageItem>>> {
    return this.http.get<ApiResponse<PaginatedResult<ContactMessageItem>>>(this.apiUrl, { params });
  }

  refreshUnread(): void {
    this.http.get<CountResponse>(`${this.apiUrl}/unread-count`).subscribe({
      next: (response) => this.unreadCount.set(response.data.count)
    });
  }

  markRead(id: number): Observable<CountResponse> {
    return this.http.put<CountResponse>(`${this.apiUrl}/${id}/read`, {}).pipe(this.applyCount());
  }

  markUnread(id: number): Observable<CountResponse> {
    return this.http.put<CountResponse>(`${this.apiUrl}/${id}/unread`, {}).pipe(this.applyCount());
  }

  markAllRead(): Observable<CountResponse> {
    return this.http.put<CountResponse>(`${this.apiUrl}/read-all`, {}).pipe(this.applyCount());
  }

  delete(id: number): Observable<CountResponse> {
    return this.http.delete<CountResponse>(`${this.apiUrl}/${id}`).pipe(this.applyCount());
  }

  private applyCount() {
    return tap((response: CountResponse) => this.unreadCount.set(response.data.count));
  }
}