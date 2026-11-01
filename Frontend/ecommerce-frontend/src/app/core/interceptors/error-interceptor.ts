import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../services/toast';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toastService = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const isLogin = req.url.includes('/auth/login');
      const isRefresh = req.url.includes('/auth/refresh-token');

      // 401 على أي طلب تاني: authInterceptor هيجدد التوكن ويعيد الطلب، فمفيش داعي نعرض رسالة
      if (error.status === 401 && !isLogin && !isRefresh) {
        return throwError(() => error);
      }

      if (isRefresh) {
        toastService.show('انتهت الجلسة، سجل الدخول مرة أخرى', 'error');
      } else if (error.status === 0) {
        toastService.show('تعذر الاتصال بالخادم، تأكد من الاتصال وحاول مرة أخرى', 'error');
      } else {
        toastService.show(error.error?.message ?? 'حصل خطأ غير متوقع، حاول مرة أخرى', 'error');
      }

      return throwError(() => error);
    })
  );
};