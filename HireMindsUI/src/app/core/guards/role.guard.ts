import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

// This is a guard factory function. It takes a required role ('Candidate' or 'Recruiter')
// and returns a standard CanActivateFn that Angular routing expects.
export const roleGuard = (expectedRole: string): CanActivateFn => {
  return (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    // If they aren't logged in at all, kick them to login
    if (!authService.isLoggedIn()) {
      router.navigate(['/login']);
      return false;
    }

    // Check if the user's role matches the required role for this route
    const userRole = authService.getUserRole();
    if (userRole === expectedRole) {
      return true;
    }

    // If they are logged in but have the wrong role, also redirect them to login
    // (Or could redirect to a 403 Unauhtorized page if we had one)
    router.navigate(['/login']);
    return false;
  };
};
