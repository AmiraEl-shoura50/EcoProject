import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { PaginatedResult } from '../models/product.model';
import { CreateReviewRequest, LatestReview, Review, ReviewSummary } from '../models/review.model';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private apiUrl = `${environment.apiUrl}/reviews`;

  constructor(private http: HttpClient) {}

  getLatest(count: number = 6): Observable<ApiResponse<LatestReview[]>> {
    return this.http.get<ApiResponse<LatestReview[]>>(`${this.apiUrl}/latest`, {
      params: { count }
    });
  }

  getByProduct(productId: number, pageNumber: number, pageSize: number): Observable<ApiResponse<PaginatedResult<Review>>> {
    return this.http.get<ApiResponse<PaginatedResult<Review>>>(`${this.apiUrl}/product/${productId}`, {
      params: { pageNumber, pageSize }
    });
  }

  getSummary(productId: number): Observable<ApiResponse<ReviewSummary>> {
    return this.http.get<ApiResponse<ReviewSummary>>(`${this.apiUrl}/product/${productId}/summary`);
  }

  create(data: CreateReviewRequest): Observable<ApiResponse<Review>> {
    return this.http.post<ApiResponse<Review>>(this.apiUrl, data);
  }
}