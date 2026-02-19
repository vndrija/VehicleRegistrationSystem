import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  UserDto,
  UpdateProfileRequest,
  ChangePasswordRequest,
  UpdateProfileResponse
} from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiConfig.authService;

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

  saveUserData(user: UserDto): void {
    localStorage.setItem('user', JSON.stringify(user));
    // Keep username for backward compatibility
    localStorage.setItem('username', user.username);
  }

  getUserData(): UserDto | null {
    const userData = localStorage.getItem('user');
    return userData ? JSON.parse(userData) : null;
  }

  getUserName(): string | null {
    return localStorage.getItem('username');
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('returnUrl');
    localStorage.removeItem('username');
    localStorage.removeItem('user');
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

  updateProfile(request: UpdateProfileRequest): Observable<UpdateProfileResponse> {
    return this.http.put<UpdateProfileResponse>(`${this.apiUrl}/profile`, request);
  }

  changePassword(request: ChangePasswordRequest): Observable<AuthResponse> {
    return this.http.put<AuthResponse>(`${this.apiUrl}/change-password`, request);
  }
}
