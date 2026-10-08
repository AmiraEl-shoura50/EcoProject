import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { LatestReview } from '../models/review.model';

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
}