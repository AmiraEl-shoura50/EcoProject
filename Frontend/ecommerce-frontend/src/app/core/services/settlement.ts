import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { PaginatedResult } from '../models/product.model';
import { CreateSettlementRequest, SellerBalance, SellerEarnings, Settlement } from '../models/settlement.model';

@Injectable({
  providedIn: 'root'
})
export class SettlementService {
  private adminUrl = `${environment.apiUrl}/admin/settlements`;
  private sellerUrl = `${environment.apiUrl}/sellers/me/earnings`;

  constructor(private http: HttpClient) {}

  // فرق توقيت المتصفح بالدقايق، عشان السيرفر يحسب حدود اليوم بتوقيتك
  private get tzOffset(): number {
    return new Date().getTimezoneOffset();
  }

  // ---- الأدمن ----
  getBalances(cutoff: string): Observable<ApiResponse<SellerBalance[]>> {
    return this.http.get<ApiResponse<SellerBalance[]>>(`${this.adminUrl}/balances`, {
      params: { cutoff, tzOffset: this.tzOffset }
    });
  }

  setCommission(sellerId: number, commissionPercent: number): Observable<unknown> {
    return this.http.put(`${this.adminUrl}/sellers/${sellerId}/commission`, { commissionPercent });
  }

  settle(sellerId: number, data: CreateSettlementRequest): Observable<ApiResponse<Settlement>> {
    return this.http.post<ApiResponse<Settlement>>(`${this.adminUrl}/sellers/${sellerId}/settle`, {
      ...data,
      tzOffset: this.tzOffset
    });
  }

  getHistory(pageNumber: number, pageSize: number): Observable<ApiResponse<PaginatedResult<Settlement>>> {
    return this.http.get<ApiResponse<PaginatedResult<Settlement>>>(`${this.adminUrl}/history`, {
      params: { pageNumber, pageSize }
    });
  }

  exportSettlement(id: number): Observable<Blob> {
    return this.http.get(`${this.adminUrl}/${id}/export`, {
      params: { tzOffset: this.tzOffset },
      responseType: 'blob'
    });
  }

  exportPeriod(from: string, to: string): Observable<Blob> {
    return this.http.get(`${this.adminUrl}/report/export`, {
      params: { from, to, tzOffset: this.tzOffset },
      responseType: 'blob'
    });
  }

  // ---- البائع ----
  getEarnings(): Observable<ApiResponse<SellerEarnings>> {
    return this.http.get<ApiResponse<SellerEarnings>>(this.sellerUrl);
  }

  getMySettlements(pageNumber: number, pageSize: number): Observable<ApiResponse<PaginatedResult<Settlement>>> {
    return this.http.get<ApiResponse<PaginatedResult<Settlement>>>(`${this.sellerUrl}/settlements`, {
      params: { pageNumber, pageSize }
    });
  }

  exportMySettlement(id: number): Observable<Blob> {
    return this.http.get(`${this.sellerUrl}/settlements/${id}/export`, {
      params: { tzOffset: this.tzOffset },
      responseType: 'blob'
    });
  }
}