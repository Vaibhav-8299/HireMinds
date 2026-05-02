import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { LoginRequest, LoginResponse, RegisterRequest } from '../models/interfaces';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = 'https://hireminds.runasp.net/api/auth';
  private readonly TOKEN_KEY = 'hireminds_jwt_token';

  constructor(private http: HttpClient) {}

  // ==========================================
  // API Calls
  // ==========================================

  // Calls the backend to register a new user
  register(dto: RegisterRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, dto);
  }

  // Verifies the 4-digit OTP sent to the user's email
  verifyEmail(email: string, otp: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/verify-email`, { email, otp }).pipe(
      tap(response => {
        if (response && response.token) {
          localStorage.setItem(this.TOKEN_KEY, response.token);
        }
      })
    );
  }

  // Calls the backend to login, and if successful, saves the token to localStorage
  login(email: string, password?: string): Observable<LoginResponse> {
    const dto: LoginRequest = { email, password };
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, dto).pipe(
      // The tap operator allows us to do a side-effect (like saving to localStorage) 
      // without modifying the actual response data being returned to the component.
      tap(response => {
        if (response && response.token) {
          localStorage.setItem(this.TOKEN_KEY, response.token);
        }
      })
    );
  }

  // Logs the user out by removing the token
  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
  }

  // ==========================================
  // Token Helpers
  // ==========================================

  // Retrieves the JWT token string from localStorage
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  // Checks if the user is currently logged in
  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;

    // Check if the token is expired
    const decoded = this.decodeToken(token);
    if (!decoded || !decoded.exp) return false;

    // JWT exp is in seconds, Date.now() is in milliseconds
    const expirationTimeMs = decoded.exp * 1000;
    const currentTimeMs = new Date().getTime();

    // True if expiration time is in the future
    return expirationTimeMs > currentTimeMs;
  }

  // ==========================================
  // Decoding JWT Claims
  // ==========================================
  // Our backend adds "UserId", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", 
  // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", and 
  // "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" to the token.

  getUserId(): number | null {
    const decoded = this.decodeToken(this.getToken());
    return decoded ? parseInt(decoded['UserId']) : null;
  }

  getUserRole(): string | null {
    const decoded = this.decodeToken(this.getToken());
    // In .NET, the standard role claim maps to this long URL key
    return decoded ? decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] : null;
  }

  getUserName(): string | null {
    const decoded = this.decodeToken(this.getToken());
    // In .NET, the standard name claim maps to this long URL key
    return decoded ? decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] : null;
  }

  getUserEmail(): string | null {
    const decoded = this.decodeToken(this.getToken());
    return decoded ? decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] : null;
  }

  // ==========================================
  // Private Helper
  // ==========================================
  
  // A simple method to decode the base64 JSON payload of the JWT token
  private decodeToken(token: string | null): any {
    if (!token) return null;
    
    try {
      // A JWT token has 3 parts separated by dots. We want the payload (middle part).
      const payload = token.split('.')[1];
      // atob() decodes base64 strings built into JS
      const decodedPayload = atob(payload);
      return JSON.parse(decodedPayload);
    } catch (e) {
      console.error('Error decoding JWT token', e);
      return null;
    }
  }
}
