import { Vehicle } from './vehicle.models';

export interface VehicleTransfer {
  id: number;
  vehicleId: number;
  fromUserId: string;
  toUserId: string;
  status: 'Pending' | 'Accepted' | 'Rejected';
  createdAt: Date | string;
  respondedAt?: Date | string;
  vehicle?: Vehicle;
}

export interface VehicleOwnershipHistory {
  id: number;
  vehicleId: number;
  ownerId: string;
  ownerName: string;
  fromDate: Date | string;
  toDate?: Date | string;
}

export interface CreateVehicleTransferDto {
  vehicleId: number;
  toUserId: string;
}

export interface RespondToTransferDto {
  accept: boolean;
}

export interface VehicleTransferResponse {
  message: string;
  data: VehicleTransfer;
}

export interface VehicleTransferListResponse {
  message: string;
  data: VehicleTransfer[];
}

export interface VehicleOwnershipHistoryResponse {
  message: string;
  data: VehicleOwnershipHistory[];
}
