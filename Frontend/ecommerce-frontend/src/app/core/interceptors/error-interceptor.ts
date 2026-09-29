import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../services/toast';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toastService = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // ✅ لو الرد فيه Message من السيرفر (بيزنس error زي "الإيميل أو الباسورد غلط")، نعرضها Toast
      // ✅ لو الرد فيه Data (يعني أخطاء Validation لكل حقل)، مش هنعرضها هنا - هي مسؤولية الفورم نفسه
      if (error.error?.message && !error.error?.data) {
        toastService.show(error.error.message, 'error');
      } else if (!error.error?.message) {
        toastService.show('حصل خطأ غير متوقع، حاول تاني', 'error');
      }

      return throwError(() => error);
    })
  );
};