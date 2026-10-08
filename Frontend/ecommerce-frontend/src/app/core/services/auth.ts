import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, RegisterRequest, AuthResponse , ForgotPasswordRequest ,ResetPasswordRequest} from '../models/auth.model';
import { ApiResponse } from '../models/api-response.model';
import { CartService } from './cart';
import { WishlistService } from './wishlist';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;

  // signal بيحمل حالة تسجيل الدخول - أي مكون في الموقع يقدر يقرأه
  isLoggedIn = signal<boolean>(this.hasToken());

  constructor(
    private http: HttpClient,
    private router: Router,
    private cartService: CartService,
    private wishlistService: WishlistService) {}

 
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
    this.cartService.cart.set(null); // ✅ نفضّي الكارت من الذاكرة بعد الخروج
  this.wishlistService.wishlist.set(null); // ✅ ونفس الحاجة للويشليست
  this.router.navigate(['/login']);
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

    // ✅ جديد - نجيب الكارت والويشليست فورًا بعد الدخول
    this.cartService.loadCart();
    this.wishlistService.loadWishlist();
  }
}

  private hasToken(): boolean {
    return !!localStorage.getItem('accessToken');
  }
  
  refreshToken(): Observable<ApiResponse<AuthResponse>> {
  const refreshToken = localStorage.getItem('refreshToken');

  return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/refresh-token`, { refreshToken }).pipe(
    tap(response => this.handleAuthSuccess(response))
  );
}

forceLogout(): void {
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
  this.isLoggedIn.set(false);
  this.cartService.cart.set(null);
  this.wishlistService.wishlist.set(null);
  this.router.navigate(['/login']);
}
}