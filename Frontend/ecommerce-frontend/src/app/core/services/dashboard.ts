import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { AdminDashboardData, SellerDashboardData } from '../models/dashboard.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  constructor(private http: HttpClient) {}

  getSeller(): Observable<ApiResponse<SellerDashboardData>> {
    return this.http.get<ApiResponse<SellerDashboardData>>(`${environment.apiUrl}/sellers/me/dashboard`, {
      params: { tzOffset: new Date().getTimezoneOffset() }
    });
  }

  getAdmin(): Observable<ApiResponse<AdminDashboardData>> {
    return this.http.get<ApiResponse<AdminDashboardData>>(`${environment.apiUrl}/admin/dashboard`, {
      params: { tzOffset: new Date().getTimezoneOffset() }
    });
  }
}