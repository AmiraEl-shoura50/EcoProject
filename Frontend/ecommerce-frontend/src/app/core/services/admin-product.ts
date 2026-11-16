import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { PaginatedResult } from '../models/product.model';
import { AdminProduct } from '../models/admin-product.model';

@Injectable({
  providedIn: 'root'
})
export class AdminProductService {
  private apiUrl = `${environment.apiUrl}/admin/products`;

  constructor(private http: HttpClient) {}

  getAll(params: {
    searchTerm: string;
    visibilityFilter: string;
    pageNumber: number;
    pageSize: number;
  }): Observable<ApiResponse<PaginatedResult<AdminProduct>>> {
    return this.http.get<ApiResponse<PaginatedResult<AdminProduct>>>(this.apiUrl, { params });
  }

  setVisibility(id: number, isHidden: boolean): Observable<unknown> {
    return this.http.put(`${this.apiUrl}/${id}/visibility`, { isHidden });
  }
}