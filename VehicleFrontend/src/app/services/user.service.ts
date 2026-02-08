import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

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
  private apiUrl = 'http://localhost:5278/api/auth';

  /**
   * Get all active users from the system
   * Used for dropdowns and user selection
   */
  getAllUsers(): Observable<GetAllUsersResponse> {
    return this.http.get<GetAllUsersResponse>(`${this.apiUrl}/users`);
  }
}
