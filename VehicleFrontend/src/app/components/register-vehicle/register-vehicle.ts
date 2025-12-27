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
  selector: 'app-register-vehicle',
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
  templateUrl: './register-vehicle.html',
  styleUrl: './register-vehicle.css',
})
export class RegisterVehicle implements OnInit {
  private fb = inject(FormBuilder);
  private vehicleService = inject(VehicleService);
  private registrationRequestService = inject(RegistrationRequestService);
  private authService = inject(AuthService);
  private messageService = inject(MessageService);
  private router = inject(Router);

  registrationForm!: FormGroup;

  userVehicles = signal<Vehicle[]>([]);
  isLoading = signal<boolean>(false);
  isLoadingVehicles = signal<boolean>(true);
  errorMessage = signal<string>('');

  insuranceDoc = signal<File | null>(null);
  inspectionDoc = signal<File | null>(null);
  identityDoc = signal<File | null>(null);

  maxDate = new Date();
  minDate = new Date();

  ngOnInit(): void {
    this.minDate.setDate(this.maxDate.getDate() - 30);

    this.registrationForm = this.fb.group({
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
        console.log('All vehicles:', response.data);
        console.log('Vehicle statuses:', response.data.map(v => ({ id: v.id, status: v.status })));

        // Filter only unregistered vehicles (or vehicles without status - treat as unregistered)
        const unregisteredVehicles = response.data.filter(v =>
          v.status === 'Unregistered' || !v.status
        );
        console.log('Unregistered vehicles:', unregisteredVehicles);

        this.userVehicles.set(unregisteredVehicles);
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

  onIdentityDocSelect(event: any): void {
    if (event.files && event.files.length > 0) {
      this.identityDoc.set(event.files[0]);
    }
  }

  submitRequest(): void {
    if (this.registrationForm.invalid) {
      Object.keys(this.registrationForm.controls).forEach(key => {
        this.registrationForm.get(key)?.markAsTouched();
      });
      return;
    }

    // Validate all documents are uploaded
    if (!this.insuranceDoc() || !this.inspectionDoc() || !this.identityDoc()) {
      this.errorMessage.set('Морате отпремити сва три документа');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');

    const formData = new FormData();
    formData.append('vehicleId', this.registrationForm.value.vehicleId.toString());
    formData.append('technicalInspectionDate', this.registrationForm.value.technicalInspectionDate.toISOString());
    formData.append('insuranceDoc', this.insuranceDoc()!);
    formData.append('inspectionDoc', this.inspectionDoc()!);
    formData.append('identityDoc', this.identityDoc()!);

    this.registrationRequestService.createRequest(formData).subscribe({
      next: (response) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Успех',
          detail: 'Захтев за регистрацију успешно поднет'
        });
        this.isLoading.set(false);

        // Reset form
        this.registrationForm.reset();
        this.insuranceDoc.set(null);
        this.inspectionDoc.set(null);
        this.identityDoc.set(null);

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
}
