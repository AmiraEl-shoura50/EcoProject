import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { AdminUser } from '../models/admin.model';
import { PaginatedResult } from '../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private apiUrl = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  getUsers(params: {
    role?: string;
    search?: string;
    pageNumber: number;
    pageSize: number;
  }): Observable<ApiResponse<PaginatedResult<AdminUser>>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber)
      .set('pageSize', params.pageSize);

    if (params.role) httpParams = httpParams.set('role', params.role);
    if (params.search) httpParams = httpParams.set('search', params.search);

    return this.http.get<ApiResponse<PaginatedResult<AdminUser>>>(`${this.apiUrl}/users`, {
      params: httpParams
    });
  }

  setUserStatus(userId: string, isActive: boolean): Observable<unknown> {
    return this.http.put(`${this.apiUrl}/users/${userId}/status`, { isActive });
  }
}