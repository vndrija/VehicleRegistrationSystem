import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TrafficViolation {
  id: number;
  registrationNumber: string;
  ownerName: string;
  violationType: string;
  location: string;
  dateOfViolation: string;
  fineAmount: number;
  status: string;
}

export interface PoliceVehicleReport {
  registrationNumber: string;
  isWanted: boolean;
  wantedReason: string | null;
  activeViolations: TrafficViolation[];
  violationCount: number;
  outstandingFines: number;
  status: 'Clear' | 'Alert' | 'Wanted';
  checkedAt: string;
}

export interface PoliceCheckResponse {
  message: string;
  data: PoliceVehicleReport;
}

@Injectable({
  providedIn: 'root'
})
export class PoliceService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5293/api/vehicles';

  /**
   * Check vehicle status with police
   * Returns if vehicle is wanted, has violations, or is clear
   */
  checkVehicleWithPolice(vehicleId: number): Observable<any> {
    return this.http.get<PoliceCheckResponse>(`${this.apiUrl}/${vehicleId}/police-check`);
  }

  /**
   * Report vehicle to police
   * Sends vehicle information to police database
   */
  reportVehicleToPolice(vehicleId: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${vehicleId}/report-to-police`, {});
  }
}
