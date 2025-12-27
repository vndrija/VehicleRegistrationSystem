import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Vehicle,
  VehicleCreateRequest,
  VehicleUpdateRequest,
  VehicleResponse,
  VehicleListResponse
} from '../models/vehicle.models';

@Injectable({
  providedIn: 'root'
})
export class VehicleService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5293/api/vehicles';

  getVehicles(): Observable<VehicleListResponse> {
    return this.http.get<VehicleListResponse>(this.apiUrl);
  }

  getVehicle(id: number): Observable<VehicleResponse> {
    return this.http.get<VehicleResponse>(`${this.apiUrl}/${id}`);
  }

  getVehiclesByOwner(ownerName: string): Observable<VehicleListResponse> {
    return this.http.get<VehicleListResponse>(`${this.apiUrl}/owner/${ownerName}`);
  }

  createVehicle(request: VehicleCreateRequest): Observable<VehicleResponse> {
    return this.http.post<VehicleResponse>(this.apiUrl, request);
  }

  updateVehicle(id: number, request: VehicleUpdateRequest): Observable<VehicleResponse> {
    return this.http.put<VehicleResponse>(`${this.apiUrl}/${id}`, request);
  }

  deleteVehicle(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
