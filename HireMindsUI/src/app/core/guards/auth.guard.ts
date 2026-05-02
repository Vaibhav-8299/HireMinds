import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // If the user is logged in, allow them to access the route
  if (authService.isLoggedIn()) {
    return true;
  }

  // Otherwise, redirect them to the login page
  router.navigate(['/login']);
  return false;
};
