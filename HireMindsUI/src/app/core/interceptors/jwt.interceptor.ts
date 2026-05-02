import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

// In Angular 15+, functional interceptors are the standard.
export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  // We use inject() to get the AuthService inside a simple function
  const authService = inject(AuthService);
  const token = authService.getToken();

  // If we have a token, clone the outgoing request and attach the Authorization header
  if (token) {
    const clonedRequest = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    
    // Pass the cloned request with the header to the next handler
    return next(clonedRequest);
  }

  // If no token exists (like during login/register), pass the original request unchanged
  return next(req);
};
