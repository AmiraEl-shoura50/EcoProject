import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = localStorage.getItem('accessToken');

  const clonedRequest = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(clonedRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      // ✅ لو الخطأ 401 ومش أصلاً طلب تسجيل دخول أو تجديد توكن، نحاول نجدد التوكن
      const isAuthRoute = req.url.includes('/auth/login') || req.url.includes('/auth/refresh-token');

      if (error.status === 401 && !isAuthRoute) {
        return authService.refreshToken().pipe(
          switchMap(() => {
            // ✅ نجح التجديد - نعيد نفس الـ request الأصلي بالتوكن الجديد
            const newToken = localStorage.getItem('accessToken');
            const retriedRequest = req.clone({
              setHeaders: { Authorization: `Bearer ${newToken}` }
            });
            return next(retriedRequest);
          }),
          catchError(() => {
            // ✅ فشل التجديد كمان - التوكن الأساسي خلاص منتهي، نعمل logout فعلي
            authService.forceLogout();
            return throwError(() => error);
          })
        );
      }

      return throwError(() => error);
    })
  );
};