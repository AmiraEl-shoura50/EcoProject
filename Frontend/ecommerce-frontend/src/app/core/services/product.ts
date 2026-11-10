import { Injectable, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { DeleteProductsResult, PaginatedResult, Product, ProductQueryParams, StockSummary } from '../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private apiUrl = `${environment.apiUrl}/products`;

  // عدد المنتجات اللي عليها عروض سارية حاليًا (الـ Navbar والرئيسية بيقروا منه)
  offersCount = signal<number>(0);

  constructor(private http: HttpClient) {}

  search(params: ProductQueryParams): Observable<ApiResponse<PaginatedResult<Product>>> {
    return this.http.get<ApiResponse<PaginatedResult<Product>>>(this.apiUrl, {
      params: this.buildParams(params)
    });
  }

  getById(id: number): Observable<ApiResponse<Product>> {
    return this.http.get<ApiResponse<Product>>(`${this.apiUrl}/${id}`);
  }

  // ---- العروض ----
  getOffers(params: ProductQueryParams): Observable<ApiResponse<PaginatedResult<Product>>> {
    return this.http.get<ApiResponse<PaginatedResult<Product>>>(`${this.apiUrl}/offers`, {
      params: this.buildParams(params)
    });
  }

  loadOffersCount(): void {
    this.getOffers({ pageNumber: 1, pageSize: 1 }).subscribe({
      next: (response) => this.offersCount.set(response.data.totalCount),
      error: () => this.offersCount.set(0)
    });
  }

  // ---- خاص بالبائع ----
  getMine(params: ProductQueryParams): Observable<ApiResponse<PaginatedResult<Product>>> {
    return this.http.get<ApiResponse<PaginatedResult<Product>>>(`${this.apiUrl}/mine`, {
      params: this.buildParams(params)
    });
  }

  create(data: FormData): Observable<ApiResponse<Product>> {
    return this.http.post<ApiResponse<Product>>(this.apiUrl, data);
  }

  update(id: number, data: FormData): Observable<unknown> {
    return this.http.put(`${this.apiUrl}/${id}`, data);
  }

  delete(id: number): Observable<unknown> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  deleteMany(productIds: number[]): Observable<ApiResponse<DeleteProductsResult>> {
    return this.http.post<ApiResponse<DeleteProductsResult>>(`${this.apiUrl}/delete-many`, { productIds });
  }

  private buildParams(params: ProductQueryParams): HttpParams {
    let httpParams = new HttpParams();

    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        httpParams = httpParams.set(key, value.toString());
      }
    });

    return httpParams;
  }
    getStockSummary(): Observable<ApiResponse<StockSummary>> {
    return this.http.get<ApiResponse<StockSummary>>(`${this.apiUrl}/mine/stock-summary`);
  }
}