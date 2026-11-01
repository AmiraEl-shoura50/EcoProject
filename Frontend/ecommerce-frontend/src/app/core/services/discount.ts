import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { CreateDiscountRequest, Discount } from '../models/discount.model';

@Injectable({
  providedIn: 'root'
})
export class DiscountService {
  private apiUrl = `${environment.apiUrl}/discounts`;

  constructor(private http: HttpClient) {}

  getMine(): Observable<ApiResponse<Discount[]>> {
    return this.http.get<ApiResponse<Discount[]>>(`${this.apiUrl}/mine`);
  }

  create(data: CreateDiscountRequest): Observable<ApiResponse<Discount>> {
    return this.http.post<ApiResponse<Discount>>(this.apiUrl, data);
  }

  delete(id: number): Observable<unknown> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}