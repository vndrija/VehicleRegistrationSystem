import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LoginRequest, RegisterRequest, AuthResponse } from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5278/api/auth';

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request);
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, request);
  }

  saveToken(token: string): void {
    localStorage.setItem('token', token);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  saveUserData(username: string): void {
    localStorage.setItem('username', username);
  }

  getUserName(): string | null {
    return localStorage.getItem('username');
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('returnUrl');
    localStorage.removeItem('username');
  }

  setReturnUrl(url: string): void {
    localStorage.setItem('returnUrl', url);
  }

  getReturnUrl(): string | null {
    return localStorage.getItem('returnUrl');
  }

  clearReturnUrl(): void {
    localStorage.removeItem('returnUrl');
  }
}
