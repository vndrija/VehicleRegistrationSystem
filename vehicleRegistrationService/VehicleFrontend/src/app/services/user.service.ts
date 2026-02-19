import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface UserOption {
  id: number;
  username: string;
  email: string;
  role: string;
}

export interface GetAllUsersResponse {
  count: number;
  data: UserOption[];
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiConfig.authService;

  /**
   * Get all active users from the system
   * Used for dropdowns and user selection
   */
  getAllUsers(): Observable<GetAllUsersResponse> {
    return this.http.get<GetAllUsersResponse>(`${this.apiUrl}/users`);
  }
}
