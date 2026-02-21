import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

// GORM serializes its embedded Model fields without json tags → uppercase keys
export interface GormModel {
  ID: number;
  CreatedAt: string;
  UpdatedAt: string;
}

export interface StolenVehicle extends GormModel {
  vehiclePlate: string;
  reportedDate: string;
  description: string;
  status: 'ACTIVE' | 'RECOVERED';
  contactInfo: string;
}

export interface Violation extends GormModel {
  vehiclePlate: string;
  officerId: number;
  type: string;
  description: string;
  location: string;
  fineAmount: number;
  status: 'PENDING' | 'PAID' | 'DISMISSED';
  violationDate: string;
  offenderEmail: string;
}

export interface Accident extends GormModel {
  location: string;
  description: string;
  severity: string;
  accidentDate: string;
  involvedPlates: string;
  isResolved: boolean;
}

export interface Officer extends GormModel {
  badgeNumber: string;
  firstName: string;
  lastName: string;
  rank: string;
  stationId: string;
  userId: string;
}

export interface VehicleFlag extends GormModel {
  vehiclePlate: string;
  flagType: string;
  description: string;
  isActive: boolean;
}

export interface VehicleDossier {
  plateNumber: string;
  isStolen: boolean;
  stolenDetails: StolenVehicle | null;
  activeWarrants: VehicleFlag[];
  unpaidViolations: Violation[];
  accidentCount: number;
  totalFinesDue: number;
}

export interface ReportStolenDto {
  vehiclePlate: string;
  description: string;
  contactInfo: string;
}

export interface ReportAccidentDto {
  location: string;
  description: string;
  severity: string;
  involvedPlates: string;
}

export interface IssueViolationDto {
  vehiclePlate: string;
  officerId: number;
  type: string;
  description: string;
  location: string;
  fineAmount: number;
  offenderEmail?: string;
}

export interface CreateOfficerDto {
  badgeNumber: string;
  firstName: string;
  lastName: string;
  rank: string;
  stationId: string;
  userId: string;
}

export interface AddFlagDto {
  vehiclePlate: string;
  flagType: string;
  description: string;
}

@Injectable({ providedIn: 'root' })
export class TrafficPoliceService {
  private http = inject(HttpClient);
  private api = environment.apiConfig.trafficPoliceService;

  // Vehicle dossier
  getVehicleStatus(plate: string): Observable<VehicleDossier> {
    return this.http.get<VehicleDossier>(`${this.api}/status/${encodeURIComponent(plate)}`);
  }

  // Stolen vehicles
  getStolenVehicles(): Observable<StolenVehicle[]> {
    return this.http.get<StolenVehicle[]>(`${this.api}/stolen`);
  }
  reportStolenVehicle(dto: ReportStolenDto): Observable<StolenVehicle> {
    return this.http.post<StolenVehicle>(`${this.api}/stolen`, dto);
  }

  // Violations
  issueViolation(dto: IssueViolationDto): Observable<Violation> {
    return this.http.post<Violation>(`${this.api}/violations`, dto);
  }
  getViolationsByPlate(plate: string): Observable<Violation[]> {
    return this.http.get<Violation[]>(`${this.api}/violations/plate/${encodeURIComponent(plate)}`);
  }
  payViolation(id: number): Observable<{ message: string; data: Violation }> {
    return this.http.put<{ message: string; data: Violation }>(`${this.api}/violations/${id}/pay`, {});
  }
  downloadViolationPdf(id: number): Observable<Blob> {
    return this.http.get(`${this.api}/violations/${id}/pdf`, { responseType: 'blob' });
  }

  // Accidents
  getAccidentsByPlate(plate: string): Observable<Accident[]> {
    return this.http.get<Accident[]>(`${this.api}/accidents/plate/${encodeURIComponent(plate)}`);
  }
  reportAccident(dto: ReportAccidentDto): Observable<any> {
    return this.http.post<any>(`${this.api}/accidents`, dto);
  }

  // Officers
  getOfficers(): Observable<Officer[]> {
    return this.http.get<Officer[]>(`${this.api}/officers`);
  }
  createOfficer(dto: CreateOfficerDto): Observable<Officer> {
    return this.http.post<Officer>(`${this.api}/officers`, dto);
  }

  // Flags
  getFlagsByPlate(plate: string): Observable<VehicleFlag[]> {
    return this.http.get<VehicleFlag[]>(`${this.api}/flags/${encodeURIComponent(plate)}`);
  }
  addFlag(dto: AddFlagDto): Observable<VehicleFlag> {
    return this.http.post<VehicleFlag>(`${this.api}/flags`, dto);
  }
  resolveFlag(id: number): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.api}/flags/${id}/resolve`, {});
  }
}
