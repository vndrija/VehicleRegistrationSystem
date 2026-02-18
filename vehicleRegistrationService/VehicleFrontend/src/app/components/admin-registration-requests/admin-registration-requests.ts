import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule  } from 'primeng/textarea';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

import { NavbarComponent } from '../navbar/navbar.component';
import { RegistrationRequestService } from '../../services/registration-request.service';
import { RegistrationRequest } from '../../models/registration-request.models';

@Component({
  selector: 'app-admin-registration-requests',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    ButtonModule,
    TableModule,
    TagModule,
    DialogModule,
    TextareaModule,
    MessageModule,
    ToastModule,
    NavbarComponent
  ],
  providers: [MessageService],
  templateUrl: './admin-registration-requests.html',
  styleUrl: './admin-registration-requests.css'
})
export class AdminRegistrationRequests implements OnInit {
  private fb = inject(FormBuilder);
  private registrationRequestService = inject(RegistrationRequestService);
  private messageService = inject(MessageService);

  requests = signal<RegistrationRequest[]>([]);
  selectedRequest = signal<RegistrationRequest | null>(null);
  isLoading = signal<boolean>(true);
  isProcessing = signal<boolean>(false);

  showRejectDialog = signal<boolean>(false);
  rejectForm!: FormGroup;

  ngOnInit(): void {
    this.rejectForm = this.fb.group({
      rejectionReason: ['', [Validators.required, Validators.minLength(10)]]
    });

    this.loadRequests();
  }

  loadRequests(status?: string): void {
    this.isLoading.set(true);
    this.registrationRequestService.getAllRequests(status).subscribe({
      next: (response) => {
        this.requests.set(response.data);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Failed to load requests', error);
        this.isLoading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Грешка',
          detail: 'Неуспешно учитавање захтева'
        });
      }
    });
  }

  filterByStatus(status: string): void {
    this.loadRequests(status);
  }

  showAllRequests(): void {
    this.loadRequests();
  }

  approveRequest(request: RegistrationRequest): void {
    this.isProcessing.set(true);
    this.registrationRequestService.reviewRequest(request.id, { approve: true }).subscribe({
      next: (response) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Успех',
          detail: 'Захтев одобрен'
        });
        this.loadRequests();
        this.isProcessing.set(false);
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Грешка',
          detail: error.error?.message || 'Неуспешно одобравање захтева'
        });
        this.isProcessing.set(false);
      }
    });
  }

  openRejectDialog(request: RegistrationRequest): void {
    this.selectedRequest.set(request);
    this.rejectForm.reset();
    this.showRejectDialog.set(true);
  }

  closeRejectDialog(): void {
    this.selectedRequest.set(null);
    this.showRejectDialog.set(false);
  }

  rejectRequest(): void {
    if (this.rejectForm.invalid) {
      this.rejectForm.get('rejectionReason')?.markAsTouched();
      return;
    }

    const request = this.selectedRequest();
    if (!request) return;

    this.isProcessing.set(true);
    this.registrationRequestService.reviewRequest(request.id, {
      approve: false,
      rejectionReason: this.rejectForm.value.rejectionReason
    }).subscribe({
      next: (response) => {
        this.messageService.add({
          severity: 'warn',
          summary: 'Одбијено',
          detail: 'Захтев одбијен'
        });
        this.loadRequests();
        this.closeRejectDialog();
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
      case 'Approved':
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
      case 'Approved':
        return 'Одобрено';
      case 'Rejected':
        return 'Одбијено';
      default:
        return status;
    }
  }

  formatDate(date: Date | string): string {
    return new Date(date).toLocaleDateString('sr-RS');
  }

  formatDateTime(date: Date | string): string {
    return new Date(date).toLocaleString('sr-RS');
  }

  getTypeSeverity(type: string): 'success' | 'info' {
    return type === 'New' ? 'success' : 'info';
  }

  getTypeLabel(type: string): string {
    return type === 'New' ? 'Нова' : 'Обнова';
  }
}
