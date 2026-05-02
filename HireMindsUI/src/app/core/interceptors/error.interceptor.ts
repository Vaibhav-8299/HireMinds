import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { ToastService } from '../services/toast.service';
import { AuthService } from '../services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const toastService = inject(ToastService);
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMsg = 'An unexpected error occurred.';

      if (error.error instanceof ErrorEvent) {
        // Client-side error
        errorMsg = error.error.message;
      } else {
        // Server-side error
        errorMsg = error.error?.message || error.statusText || 'Server Error';

        if (error.status === 401) {
          errorMsg = 'Session expired or unauthorized. Please log in again.';
          authService.logout();
          router.navigate(['/login']);
        }
      }

      // Automatically show the global error toast!
      toastService.showError(errorMsg);

      return throwError(() => error);
    })
  );
};
