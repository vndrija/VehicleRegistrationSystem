import { Vehicle } from './vehicle.models';

export interface RegistrationRequest {
  id: number;
  vehicleId: number;
  userId: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  technicalInspectionDate: Date | string;
  insuranceDocPath: string;
  inspectionDocPath: string;
  identityDocPath: string;
  createdAt: Date | string;
  reviewedAt?: Date | string;
  reviewedBy?: string;
  rejectionReason?: string;
  vehicle?: Vehicle;
}

export interface CreateRegistrationRequestDto {
  vehicleId: number;
  technicalInspectionDate: Date | string;
  insuranceDoc: File;
  inspectionDoc: File;
  identityDoc: File;
}

export interface ReviewRegistrationRequestDto {
  approve: boolean;
  rejectionReason?: string;
}

export interface RegistrationRequestResponse {
  message: string;
  data: RegistrationRequest;
}

export interface RegistrationRequestListResponse {
  message: string;
  data: RegistrationRequest[];
}
