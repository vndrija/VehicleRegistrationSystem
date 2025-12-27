import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  RegistrationRequest,
  RegistrationRequestResponse,
  RegistrationRequestListResponse,
  ReviewRegistrationRequestDto
} from '../models/registration-request.models';

@Injectable({
  providedIn: 'root'
})
export class RegistrationRequestService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5293/api/RegistrationRequests';

  createRequest(formData: FormData): Observable<RegistrationRequestResponse> {
    return this.http.post<RegistrationRequestResponse>(this.apiUrl, formData);
  }

  getMyRequests(): Observable<RegistrationRequestListResponse> {
    return this.http.get<RegistrationRequestListResponse>(`${this.apiUrl}/my-requests`);
  }

  getAllRequests(status?: string): Observable<RegistrationRequestListResponse> {
    const url = status ? `${this.apiUrl}?status=${status}` : this.apiUrl;
    return this.http.get<RegistrationRequestListResponse>(url);
  }

  getRequestById(id: number): Observable<RegistrationRequestResponse> {
    return this.http.get<RegistrationRequestResponse>(`${this.apiUrl}/${id}`);
  }

  reviewRequest(id: number, review: ReviewRegistrationRequestDto): Observable<RegistrationRequestResponse> {
    return this.http.post<RegistrationRequestResponse>(`${this.apiUrl}/${id}/review`, review);
  }
}
