import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  ChangePasswordRequest
} from '../models/auth.model';
import { ApiResponse } from '../models/api-response.model';
import { CartService } from './cart';
import { WishlistService } from './wishlist';
import { NotificationService } from './notification';
// ✅ جديد: فوق الكلاس ومعمول export عشان roleGuard يستورده
export type UserRole = 'Customer' | 'Seller' | 'Admin';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;

  isLoggedIn = signal<boolean>(this.hasToken());

  // ✅ جديد: تحت isLoggedIn مباشرة
  userRole = signal<UserRole | null>(this.readRole());

  constructor(
    private http: HttpClient,
    private router: Router,
    private cartService: CartService,
    private wishlistService: WishlistService,
    private notificationService: NotificationService
  ) {}

  register(data: RegisterRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/register`, data)
      .pipe(tap(response => this.handleAuthSuccess(response)));
  }

  login(data: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/login`, data)
      .pipe(tap(response => this.handleAuthSuccess(response)));
  }

  refreshToken(): Observable<ApiResponse<AuthResponse>> {
    const refreshToken = localStorage.getItem('refreshToken');

    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/refresh-token`, { refreshToken })
      .pipe(tap(response => this.handleAuthSuccess(response)));
  }

  logout(): void {
    const refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken) {
      this.http.post(`${this.apiUrl}/revoke-token`, { refreshToken }).subscribe();
    }
    this.clearSession();
  }

  forceLogout(): void {
    this.clearSession();
  }

  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  forgotPassword(data: ForgotPasswordRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/forgot-password`, data);
  }

  resetPassword(data: ResetPasswordRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/reset-password`, data);
  }

  changePassword(data: ChangePasswordRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/change-password`, data)
      .pipe(tap(response => this.handleAuthSuccess(response)));
  }

  private handleAuthSuccess(response: ApiResponse<AuthResponse>): void {
    if (response.success && response.data.token) {
      localStorage.setItem('accessToken', response.data.token);
      localStorage.setItem('refreshToken', response.data.refreshToken!);
      this.isLoggedIn.set(true);
      this.userRole.set(this.readRole()); // ✅ جديد
      this.notificationService.start(); // الإشعارات لكل الأدوار
      // ✅ السلة والمفضلة للعملاء بس
      if (this.userRole() === 'Customer') {
        this.cartService.loadCart();
        this.wishlistService.loadWishlist();
      }
    }
  }

  // ✅ جديد: تنضيف الجلسة في مكان واحد بدل التكرار في logout و forceLogout
  private clearSession(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    this.isLoggedIn.set(false);
    this.userRole.set(null); // ✅ جديد
    this.notificationService.stop();
    this.cartService.cart.set(null);
    this.wishlistService.wishlist.set(null);
    this.router.navigate(['/login']);
  }

  private hasToken(): boolean {
    return !!localStorage.getItem('accessToken');
  }

  // ✅ جديد: جنب hasToken() في آخر الكلاس
  // بتفك الـ JWT (الجزء الأوسط منه) وتجيب الدور
  private readRole(): UserRole | null {
    const token = localStorage.getItem('accessToken');
    if (!token) return null;

    try {
      const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
      const json = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + c.charCodeAt(0).toString(16).padStart(2, '0'))
          .join('')
      );
      const payload = JSON.parse(json);

      const raw = payload['role'] ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      const role = Array.isArray(raw) ? raw[0] : raw;

      return role === 'Customer' || role === 'Seller' || role === 'Admin' ? role : null;
    } catch {
      return null;
    }
  }
}