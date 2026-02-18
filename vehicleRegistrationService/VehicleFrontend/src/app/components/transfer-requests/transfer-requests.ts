import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

import { NavbarComponent } from '../navbar/navbar.component';
import { VehicleTransferService } from '../../services/vehicle-transfer.service';
import { AuthService } from '../../services/auth.service';
import { VehicleTransfer } from '../../models/vehicle-transfer.models';

@Component({
  selector: 'app-transfer-requests',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    TableModule,
    TagModule,
    ToastModule,
    NavbarComponent
  ],
  providers: [MessageService],
  templateUrl: './transfer-requests.html',
  styleUrl: './transfer-requests.css'
})
export class TransferRequests implements OnInit {
  private vehicleTransferService = inject(VehicleTransferService);
  private authService = inject(AuthService);
  private messageService = inject(MessageService);
  private router = inject(Router);

  pendingReceivedTransfers = signal<VehicleTransfer[]>([]);
  allTransfers = signal<VehicleTransfer[]>([]);
  isLoadingPending = signal<boolean>(true);
  isLoadingAll = signal<boolean>(true);
  isProcessing = signal<boolean>(false);

  ngOnInit(): void {
    const user = this.authService.getUserData();
    if (!user) {
      this.router.navigate(['/login']);
      return;
    }

    this.loadPendingReceivedTransfers();
    this.loadAllTransfers();
  }

  loadPendingReceivedTransfers(): void {
    this.isLoadingPending.set(true);
    this.vehicleTransferService.getPendingReceivedTransfers().subscribe({
      next: (response) => {
        this.pendingReceivedTransfers.set(response.data);
        this.isLoadingPending.set(false);
      },
      error: (error) => {
        console.error('Failed to load pending received transfers', error);
        this.isLoadingPending.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Грешка',
          detail: 'Неуспешно учитавање захтева'
        });
      }
    });
  }

  loadAllTransfers(): void {
    this.isLoadingAll.set(true);
    this.vehicleTransferService.getMyTransfers().subscribe({
      next: (response) => {
        this.allTransfers.set(response.data);
        this.isLoadingAll.set(false);
      },
      error: (error) => {
        console.error('Failed to load all transfers', error);
        this.isLoadingAll.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Грешка',
          detail: 'Неуспешно учитавање историје'
        });
      }
    });
  }

  acceptTransfer(transfer: VehicleTransfer): void {
    this.isProcessing.set(true);
    this.vehicleTransferService.respondToTransfer(transfer.id, { accept: true }).subscribe({
      next: (response) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Успех',
          detail: 'Захтев прихваћен. Возило је сада ваше!'
        });
        this.loadPendingReceivedTransfers();
        this.loadAllTransfers();
        this.isProcessing.set(false);
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Грешка',
          detail: error.error?.message || 'Неуспешно прихватање захтева'
        });
        this.isProcessing.set(false);
      }
    });
  }

  rejectTransfer(transfer: VehicleTransfer): void {
    this.isProcessing.set(true);
    this.vehicleTransferService.respondToTransfer(transfer.id, { accept: false }).subscribe({
      next: (response) => {
        this.messageService.add({
          severity: 'warn',
          summary: 'Одбијено',
          detail: 'Захтев одбијен'
        });
        this.loadPendingReceivedTransfers();
        this.loadAllTransfers();
        this.isProcessing.set(false);
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Грешка',
          detail: error.error?.message || 'Неуспешно одбијање захтева'
        });
        this.isProcessing.set(false);
      }
    });
  }

  getStatusSeverity(status: string): 'success' | 'warn' | 'danger' | 'info' {
    switch (status) {
      case 'Accepted':
        return 'success';
      case 'Rejected':
        return 'danger';
      case 'Pending':
        return 'warn';
      default:
        return 'info';
    }
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'Pending':
        return 'На чекању';
      case 'Accepted':
        return 'Прихваћено';
      case 'Rejected':
        return 'Одбијено';
      default:
        return status;
    }
  }

  formatDateTime(date: Date | string): string {
    return new Date(date).toLocaleString('sr-RS');
  }
}
