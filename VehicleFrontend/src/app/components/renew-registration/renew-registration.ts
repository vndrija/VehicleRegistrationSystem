import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { FileUploadModule } from 'primeng/fileupload';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

import { NavbarComponent } from '../navbar/navbar.component';
import { VehicleService } from '../../services/vehicle.service';
import { RegistrationRequestService } from '../../services/registration-request.service';
import { AuthService } from '../../services/auth.service';
import { Vehicle } from '../../models/vehicle.models';

@Component({
  selector: 'app-renew-registration',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    MessageModule,
    SelectModule,
    DatePickerModule,
    FileUploadModule,
    ToastModule,
    NavbarComponent
  ],
  providers: [MessageService],
  templateUrl: './renew-registration.html',
  styleUrl: './renew-registration.css',
})
export class RenewRegistration implements OnInit {
  private fb = inject(FormBuilder);
  private vehicleService = inject(VehicleService);
  private registrationRequestService = inject(RegistrationRequestService);
  private authService = inject(AuthService);
  private messageService = inject(MessageService);
  private router = inject(Router);

  renewalForm!: FormGroup;

  userVehicles = signal<Vehicle[]>([]);
  isLoading = signal<boolean>(false);
  isLoadingVehicles = signal<boolean>(true);
  errorMessage = signal<string>('');

  insuranceDoc = signal<File | null>(null);
  inspectionDoc = signal<File | null>(null);

  maxDate = new Date();
  minDate = new Date();

  ngOnInit(): void {
    this.minDate.setDate(this.maxDate.getDate() - 30);

    this.renewalForm = this.fb.group({
      vehicleId: ['', [Validators.required]],
      technicalInspectionDate: ['', [Validators.required]]
    });

    this.loadUserVehicles();
  }

  loadUserVehicles(): void {
    const user = this.authService.getUserData();
    if (!user) {
      this.router.navigate(['/login']);
      return;
    }

    this.isLoadingVehicles.set(true);
    this.vehicleService.getVehiclesByOwner(user.username).subscribe({
      next: (response) => {
        // Filter vehicles that are Registered AND (expire within 30 days OR already expired)
        const today = new Date();
        const in30Days = new Date();
        in30Days.setDate(today.getDate() + 30);

        const renewableVehicles = response.data.filter(v => {
          // Must be registered
          if (v.status !== 'Registered') return false;

          const expirationDate = new Date(v.expirationDate);
          const isExpired = expirationDate < today;
          const expiresWithin30Days = expirationDate <= in30Days && expirationDate >= today;

          return isExpired || expiresWithin30Days;
        });

        this.userVehicles.set(renewableVehicles);
        this.isLoadingVehicles.set(false);
      },
      error: (error) => {
        console.error('Failed to load vehicles', error);
        this.isLoadingVehicles.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Грешка',
          detail: 'Неуспешно учитавање возила'
        });
      }
    });
  }

  onInsuranceDocSelect(event: any): void {
    if (event.files && event.files.length > 0) {
      this.insuranceDoc.set(event.files[0]);
    }
  }

  onInspectionDocSelect(event: any): void {
    if (event.files && event.files.length > 0) {
      this.inspectionDoc.set(event.files[0]);
    }
  }

  submitRequest(): void {
    if (this.renewalForm.invalid) {
      Object.keys(this.renewalForm.controls).forEach(key => {
        this.renewalForm.get(key)?.markAsTouched();
      });
      return;
    }

    // Validate both documents are uploaded
    if (!this.insuranceDoc() || !this.inspectionDoc()) {
      this.errorMessage.set('Морате отпремити оба документа (Осигурање и Технички преглед)');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');

    const formData = new FormData();
    formData.append('vehicleId', this.renewalForm.value.vehicleId.toString());
    formData.append('type', 'Renewal'); // Set type to Renewal
    formData.append('technicalInspectionDate', this.renewalForm.value.technicalInspectionDate.toISOString());
    formData.append('insuranceDoc', this.insuranceDoc()!);
    formData.append('inspectionDoc', this.inspectionDoc()!);
    // No identity document for renewal

    this.registrationRequestService.createRequest(formData).subscribe({
      next: (response) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Успех',
          detail: 'Захтев за обнову регистрације успешно поднет'
        });
        this.isLoading.set(false);

        // Reset form
        this.renewalForm.reset();
        this.insuranceDoc.set(null);
        this.inspectionDoc.set(null);

        // Redirect to profile after 2 seconds
        setTimeout(() => {
          this.router.navigate(['/profile']);
        }, 2000);
      },
      error: (error) => {
        this.errorMessage.set(error.error?.message || 'Неуспешно подношење захтева');
        this.isLoading.set(false);
      }
    });
  }

  formatExpirationDate(date: Date | string): string {
    return new Date(date).toLocaleDateString('sr-RS');
  }

  isExpired(date: Date | string): boolean {
    return new Date(date) < new Date();
  }
}
