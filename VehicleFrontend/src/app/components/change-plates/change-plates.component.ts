import { Component, OnInit, inject, signal, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { ToastModule } from 'primeng/toast';
import { SelectModule } from 'primeng/select';
import { MessageService } from 'primeng/api';

import { VehicleService } from '../../services/vehicle.service';
import { AuthService } from '../../services/auth.service';
import { NavbarComponent } from '../navbar/navbar.component';
import { SignaturePadComponent } from '../signature-pad/signature-pad.component';
import { Vehicle } from '../../models/vehicle.models';

@Component({
  selector: 'app-change-plates',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    MessageModule,
    ToastModule,
    SelectModule,
    NavbarComponent,
    SignaturePadComponent
  ],
  providers: [MessageService],
  template: `
    <app-navbar></app-navbar>
    <p-toast></p-toast>

    <div class="min-h-screen bg-gray-50 py-8">
      <div class="max-w-2xl mx-auto px-4">
        <h1 class="text-3xl font-bold text-gray-900 mb-8">Промена регистарске таблице</h1>

        <p-card>
          <ng-template pTemplate="header">
            <div class="p-4 bg-blue-600 text-white">
              <h2 class="text-xl font-semibold">Унесите детаље за промену таблице</h2>
            </div>
          </ng-template>

          <form [formGroup]="changePlateForm" class="space-y-6">
            <!-- Vehicle Selection -->
            <div>
              <label for="vehicleId" class="block text-sm font-medium text-gray-700 mb-2">
                Изаберите возило *
              </label>
              @if (isLoadingVehicles()) {
                <div class="flex items-center gap-2 p-3 border border-gray-300 rounded">
                  <i class="pi pi-spin pi-spinner text-blue-600"></i>
                  <span class="text-gray-600">Учитавање возила...</span>
                </div>
              } @else {
                <p-select
                  id="vehicleId"
                  [options]="userVehicles()"
                  formControlName="vehicleId"
                  optionLabel="displayName"
                  optionValue="id"
                  placeholder="Изаберите возило"
                  class="w-full"
                  [showClear]="true"
                  [filter]="true"
                ></p-select>
              }
              @if (changePlateForm.get('vehicleId')?.invalid && changePlateForm.get('vehicleId')?.touched) {
                <small class="text-red-600">Возило је обавезно</small>
              }
            </div>

            <!-- Current Registration Number (Read-only) -->
            @if (selectedVehicle()) {
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">
                  Тренутна регистарска таблица
                </label>
                <input
                  type="text"
                  pInputText
                  [value]="selectedVehicle()?.registrationNumber"
                  class="w-full bg-gray-100"
                  readonly
                />
              </div>
            }

            <!-- New Registration Number -->
            <div>
              <label for="newRegistration" class="block text-sm font-medium text-gray-700 mb-2">
                Нова регистарска таблица *
              </label>
              <input
                id="newRegistration"
                type="text"
                pInputText
                formControlName="newRegistrationNumber"
                placeholder="нпр. BG-1234-AB"
                class="w-full"
              />
              <small class="text-gray-500">Формат: BG-1234-AB (2 слова - 4 цифре - 2 слова)</small>
              @if (changePlateForm.get('newRegistrationNumber')?.invalid && changePlateForm.get('newRegistrationNumber')?.touched) {
                <small class="text-red-600 block">Регистарска таблица је обавезна</small>
              }
            </div>

            <!-- Reason -->
            <div>
              <label for="reason" class="block text-sm font-medium text-gray-700 mb-2">
                Разлог за промену *
              </label>
              <p-select
                id="reason"
                [options]="reasons"
                formControlName="reason"
                placeholder="Изаберите разлог"
                class="w-full"
              ></p-select>
              @if (changePlateForm.get('reason')?.invalid && changePlateForm.get('reason')?.touched) {
                <small class="text-red-600">Разлог је обавезан</small>
              }
            </div>

            <!-- Signature Pad -->
            <div class="border-t pt-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-4">Верификација</h3>
              <app-signature-pad
                (signatureCaptured)="onSignatureCaptured($event)"
              ></app-signature-pad>
            </div>

            <!-- Error Messages -->
            @if (error()) {
              <p-message severity="error" [text]="error()"></p-message>
            }

            <!-- Submit Button -->
            <div class="flex gap-2 pt-4">
              <p-button
                label="Промени табличу"
                icon="pi pi-check"
                [loading]="isSubmitting()"
                (onClick)="changeLicensePlate()"
              ></p-button>
              <p-button
                label="Откажи"
                icon="pi pi-times"
                severity="secondary"
                (onClick)="cancel()"
              ></p-button>
            </div>
          </form>
        </p-card>

        <!-- Info Box -->
        <p-card class="mt-8">
          <ng-template pTemplate="header">
            <div class="p-4 bg-blue-100">
              <h3 class="text-lg font-semibold text-blue-900">Информације</h3>
            </div>
          </ng-template>
          <div class="space-y-3 text-gray-700">
            <p><i class="pi pi-info-circle mr-2 text-blue-600"></i><strong>Нова таблица:</strong> Регистарска таблица мора бити јединствена у систему</p>
            <p><i class="pi pi-info-circle mr-2 text-blue-600"></i><strong>Разлог:</strong> Неопходно је навести разлог за промену</p>
            <p><i class="pi pi-info-circle mr-2 text-blue-600"></i><strong>Потврда:</strong> Промена ће бити забележена у вашој историји возила</p>
          </div>
        </p-card>
      </div>
    </div>
  `,
  styles: [`
    :host ::ng-deep {
      .p-select {
        width: 100%;
      }
    }
  `]
})
export class ChangePlatesComponent implements OnInit {
  @ViewChild(SignaturePadComponent) signaturePad!: SignaturePadComponent;

  private fb = inject(FormBuilder);
  private vehicleService = inject(VehicleService);
  private messageService = inject(MessageService);
  private router = inject(Router);
  private authService = inject(AuthService);

  changePlateForm!: FormGroup;
  userVehicles = signal<Vehicle[]>([]);
  selectedVehicle = signal<Vehicle | null>(null);
  isLoadingVehicles = signal(false);
  isSubmitting = signal(false);
  isSignatureCaptured = signal(false);
  error = signal<string>('');

  reasons = [
    { label: 'Оштећена таблица', value: 'Damaged' },
    { label: 'Изгубљена таблица', value: 'Lost' },
    { label: 'Украдена таблица', value: 'Stolen' },
    { label: 'Лична преференца', value: 'Owner preference' }
  ];

  ngOnInit(): void {
    this.initializeForm();
    this.loadUserVehicles();
  }

  initializeForm(): void {
    this.changePlateForm = this.fb.group({
      vehicleId: ['', Validators.required],
      newRegistrationNumber: ['', [Validators.required, Validators.pattern(/^[A-Z]{2}-\d{4}-[A-Z]{2}$/)]],
      reason: ['', Validators.required]
    });

    // Track vehicle selection
    this.changePlateForm.get('vehicleId')?.valueChanges.subscribe((vehicleId) => {
      const vehicle = this.userVehicles().find(v => v.id === vehicleId);
      this.selectedVehicle.set(vehicle || null);
    });
  }

  loadUserVehicles(): void {
    this.isLoadingVehicles.set(true);
    const currentUser = this.authService.getUserData();

    if (!currentUser) {
      this.error.set('Корисник није аутентификован');
      this.isLoadingVehicles.set(false);
      return;
    }

    this.vehicleService.getVehiclesByOwnerId(String(currentUser.id)).subscribe({
      next: (response) => {
        // Add displayName property for dropdown
        const vehiclesWithDisplay = response.data.map((v: any) => ({
          ...v,
          displayName: `${v.make} ${v.model} (${v.registrationNumber})`
        }));
        this.userVehicles.set(vehiclesWithDisplay);
        this.isLoadingVehicles.set(false);
      },
      error: (err) => {
        this.error.set('Неуспешно учитавање возила');
        this.isLoadingVehicles.set(false);
      }
    });
  }

  changeLicensePlate(): void {
    if (this.changePlateForm.invalid) {
      Object.keys(this.changePlateForm.controls).forEach(key => {
        this.changePlateForm.get(key)?.markAsTouched();
      });
      return;
    }

    if (!this.isSignatureCaptured()) {
      this.error.set('Молим вас да потпишете пре него што наставите');
      return;
    }

    const vehicleId = this.changePlateForm.value.vehicleId;
    if (!vehicleId) return;

    this.isSubmitting.set(true);
    this.error.set('');

    const request = {
      newRegistrationNumber: this.changePlateForm.value.newRegistrationNumber,
      reason: this.changePlateForm.value.reason
    };

    this.vehicleService.changeLicensePlate(vehicleId, request).subscribe({
      next: (response) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Успех',
          detail: 'Регистарска таблица је успешно промењена',
          life: 3000
        });
        this.isSubmitting.set(false);
        setTimeout(() => {
          this.router.navigate(['/profile']);
        }, 2000);
      },
      error: (error) => {
        this.error.set(error.error?.message || 'Неуспешна промена регистарске таблице');
        this.isSubmitting.set(false);
      }
    });
  }

  onSignatureCaptured(signatureData: string): void {
    this.isSignatureCaptured.set(!!signatureData);
    this.error.set('');
  }

  cancel(): void {
    this.router.navigate(['/']);
  }
}
