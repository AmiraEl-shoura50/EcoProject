import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, RegisterRequest, AuthResponse , ForgotPasswordRequest ,ResetPasswordRequest} from '../models/auth.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;

  // signal بيحمل حالة تسجيل الدخول - أي مكون في الموقع يقدر يقرأه
  isLoggedIn = signal<boolean>(this.hasToken());

  constructor(private http: HttpClient, private router: Router) {}

  register(data: RegisterRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/register`, data)
      .pipe(
        tap(response => this.handleAuthSuccess(response))
      );
  }

  login(data: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/login`, data)
      .pipe(
        tap(response => this.handleAuthSuccess(response))
      );
  }
forgotPassword(data: ForgotPasswordRequest): Observable<ApiResponse<AuthResponse>> {
  return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/forgot-password`, data);
}

resetPassword(data: ResetPasswordRequest): Observable<ApiResponse<AuthResponse>> {
  return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/reset-password`, data);
}
  logout(): void {
    const refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken) {
      this.http.post(`${this.apiUrl}/revoke-token`, { refreshToken }).subscribe();
    }
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    this.isLoggedIn.set(false);
    this.router.navigate(['/login']);
  }

  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  private handleAuthSuccess(response: ApiResponse<AuthResponse>): void {
    if (response.success && response.data.token) {
      localStorage.setItem('accessToken', response.data.token);
      localStorage.setItem('refreshToken', response.data.refreshToken!);
      this.isLoggedIn.set(true);
    }
  }

  private hasToken(): boolean {
    return !!localStorage.getItem('accessToken');
  }
}