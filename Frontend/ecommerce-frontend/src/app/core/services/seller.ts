import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { Seller, SellerProfile } from '../models/seller.model';

@Injectable({
  providedIn: 'root'
})
export class SellerService {
  private apiUrl = `${environment.apiUrl}/sellers`;

  constructor(private http: HttpClient) {}

  getById(id: number): Observable<ApiResponse<Seller>> {
    return this.http.get<ApiResponse<Seller>>(`${this.apiUrl}/${id}`);
  }

  getMine(): Observable<ApiResponse<SellerProfile>> {
    return this.http.get<ApiResponse<SellerProfile>>(`${this.apiUrl}/me`);
  }

  update(data: FormData): Observable<unknown> {
    return this.http.put(`${this.apiUrl}/me`, data);
  }
}