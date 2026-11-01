import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService, UserRole } from '../services/auth';

// دالة بترجّع Guard مخصص لدور معين: roleGuard('Seller')
export const roleGuard = (role: UserRole): CanActivateFn => (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isLoggedIn()) {
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }

  return authService.userRole() === role ? true : router.createUrlTree(['/']);
};