import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  VehicleTransferResponse,
  VehicleTransferListResponse,
  VehicleOwnershipHistoryResponse,
  CreateVehicleTransferDto,
  RespondToTransferDto
} from '../models/vehicle-transfer.models';

@Injectable({
  providedIn: 'root'
})
export class VehicleTransferService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiConfig.vehicleTransfers;

  createTransfer(dto: CreateVehicleTransferDto): Observable<VehicleTransferResponse> {
    return this.http.post<VehicleTransferResponse>(this.apiUrl, dto);
  }

  getMyTransfers(): Observable<VehicleTransferListResponse> {
    return this.http.get<VehicleTransferListResponse>(`${this.apiUrl}/my-requests`);
  }

  getTransferById(id: number): Observable<VehicleTransferResponse> {
    return this.http.get<VehicleTransferResponse>(`${this.apiUrl}/${id}`);
  }

  respondToTransfer(id: number, dto: RespondToTransferDto): Observable<VehicleTransferResponse> {
    return this.http.post<VehicleTransferResponse>(`${this.apiUrl}/${id}/respond`, dto);
  }

  getPendingReceivedTransfers(): Observable<VehicleTransferListResponse> {
    return this.http.get<VehicleTransferListResponse>(`${this.apiUrl}/pending-received`);
  }

  getOwnershipHistory(vehicleId: number): Observable<VehicleOwnershipHistoryResponse> {
    return this.http.get<VehicleOwnershipHistoryResponse>(`http://localhost:5293/api/Vehicles/${vehicleId}/ownership-history`);
  }
}
