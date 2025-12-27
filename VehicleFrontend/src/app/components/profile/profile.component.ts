import { Component, inject, signal, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { MessageModule } from 'primeng/message';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

import { AuthService } from '../../services/auth.service';
import { VehicleService } from '../../services/vehicle.service';
import { NavbarComponent } from '../navbar/navbar.component';
import { UserDto, UpdateProfileRequest, ChangePasswordRequest } from '../../models/auth.models';
import { Vehicle } from '../../models/vehicle.models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    PasswordModule,
    MessageModule,
    DialogModule,
    ConfirmDialogModule,
    ToastModule,
    NavbarComponent
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private vehicleService = inject(VehicleService);
  private router = inject(Router);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  currentUser = signal<UserDto | null>(null);

  profileForm!: FormGroup;
  passwordForm!: FormGroup;

  isEditingProfile = signal<boolean>(false);
  isChangingPassword = signal<boolean>(false);
  isLoadingProfile = signal<boolean>(false);
  isLoadingPassword = signal<boolean>(false);
  isLoadingVehicles = signal<boolean>(true);

  userVehicles = signal<Vehicle[]>([]);
  selectedVehicle = signal<Vehicle | null>(null);
  showEditDialog = signal<boolean>(false);

  profileError = signal<string>('');
  passwordError = signal<string>('');

  constructor() {
    this.initializeForms();
  }

  ngOnInit(): void {
    this.loadUserData();
    this.loadUserVehicles();
  }

  private initializeForms(): void {
    this.profileForm = this.fb.group({
      username: [{ value: '', disabled: true }, [Validators.required, Validators.minLength(3)]],
      email: [{ value: '', disabled: true }, [Validators.required, Validators.email]],
      role: [{ value: '', disabled: true }]
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  private passwordMatchValidator(form: FormGroup): { [key: string]: boolean } | null {
    const newPassword = form.get('newPassword');
    const confirmPassword = form.get('confirmPassword');

    if (newPassword && confirmPassword && newPassword.value !== confirmPassword.value) {
      return { passwordMismatch: true };
    }
    return null;
  }

  private loadUserData(): void {
    const user = this.authService.getUserData();
    if (user) {
      this.currentUser.set(user);
      this.profileForm.patchValue({
        username: user.username,
        email: user.email,
        role: user.role
      });
    } else {
      this.router.navigate(['/login']);
    }
  }

  private loadUserVehicles(): void {
    const user = this.currentUser();
    if (!user) return;

    this.isLoadingVehicles.set(true);
    this.vehicleService.getVehiclesByOwner(user.username).subscribe({
      next: (response) => {
        this.userVehicles.set(response.data);
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

  enableProfileEdit(): void {
    this.isEditingProfile.set(true);
    this.profileForm.get('username')?.enable();
    this.profileForm.get('email')?.enable();
  }

  cancelProfileEdit(): void {
    this.isEditingProfile.set(false);
    this.loadUserData();
    this.profileForm.get('username')?.disable();
    this.profileForm.get('email')?.disable();
    this.profileError.set('');
  }

  saveProfile(): void {
    if (this.profileForm.invalid) return;

    this.isLoadingProfile.set(true);
    this.profileError.set('');

    const request: UpdateProfileRequest = {
      username: this.profileForm.value.username,
      email: this.profileForm.value.email
    };

    this.authService.updateProfile(request).subscribe({
      next: (response) => {
        this.authService.saveUserData(response.data);
        this.currentUser.set(response.data);
        this.isEditingProfile.set(false);
        this.profileForm.get('username')?.disable();
        this.profileForm.get('email')?.disable();
        this.isLoadingProfile.set(false);
        this.messageService.add({
          severity: 'success',
          summary: 'Успех',
          detail: 'Профил успешно ажуриран'
        });
      },
      error: (error) => {
        this.profileError.set(error.error?.message || 'Неуспешна измена профила');
        this.isLoadingProfile.set(false);
      }
    });
  }

  togglePasswordChange(): void {
    this.isChangingPassword.update(value => !value);
    if (!this.isChangingPassword()) {
      this.passwordForm.reset();
      this.passwordError.set('');
    }
  }

  changePassword(): void {
    if (this.passwordForm.invalid) return;

    this.isLoadingPassword.set(true);
    this.passwordError.set('');

    const request: ChangePasswordRequest = {
      currentPassword: this.passwordForm.value.currentPassword,
      newPassword: this.passwordForm.value.newPassword
    };

    this.authService.changePassword(request).subscribe({
      next: (response) => {
        this.passwordForm.reset();
        this.isChangingPassword.set(false);
        this.isLoadingPassword.set(false);
        this.messageService.add({
          severity: 'success',
          summary: 'Успех',
          detail: 'Лозинка успешно промењена'
        });
      },
      error: (error) => {
        this.passwordError.set(error.error?.message || 'Неуспешна промена лозинке');
        this.isLoadingPassword.set(false);
      }
    });
  }

  isVehicleExpiringSoon(vehicle: Vehicle): boolean {
    const expirationDate = new Date(vehicle.expirationDate);
    const today = new Date();
    const daysUntilExpiration = Math.ceil((expirationDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
    return daysUntilExpiration <= 30 && daysUntilExpiration >= 0;
  }

  isVehicleExpired(vehicle: Vehicle): boolean {
    const expirationDate = new Date(vehicle.expirationDate);
    const today = new Date();
    return expirationDate < today;
  }

  openEditDialog(vehicle: Vehicle): void {
    this.selectedVehicle.set(vehicle);
    this.showEditDialog.set(true);
  }

  closeEditDialog(): void {
    this.selectedVehicle.set(null);
    this.showEditDialog.set(false);
  }

  deleteVehicle(vehicle: Vehicle): void {
    this.confirmationService.confirm({
      message: `Да ли сте сигурни да желите да обришете возило ${vehicle.registrationNumber}?`,
      header: 'Потврда брисања',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.vehicleService.deleteVehicle(vehicle.id).subscribe({
          next: () => {
            this.userVehicles.update(vehicles =>
              vehicles.filter(v => v.id !== vehicle.id)
            );
            this.messageService.add({
              severity: 'success',
              summary: 'Успех',
              detail: 'Возило успешно обрисано'
            });
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Грешка',
              detail: 'Неуспешно брисање возила'
            });
          }
        });
      }
    });
  }

  formatDate(date: Date | string): string {
    return new Date(date).toLocaleDateString('sr-RS');
  }
}
