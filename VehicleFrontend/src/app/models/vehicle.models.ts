export interface Vehicle {
  id: number;
  registrationNumber: string;
  make: string;
  model: string;
  year: number;
  ownerName: string;
  expirationDate: Date | string;
  status: 'Unregistered' | 'Registered' | 'Active' | 'Deregistered';
}

export interface VehicleCreateRequest {
  registrationNumber: string;
  make: string;
  model: string;
  year: number;
}

export interface VehicleUpdateRequest {
  make: string;
  model: string;
  year: number;

}

export interface VehicleResponse {
  message: string;
  data: Vehicle;
}

export interface VehicleListResponse {
  message: string;
  data: Vehicle[];
}
