import { Component, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';

import { AuthService } from '../../services/auth.service';
import { VehicleService } from '../../services/vehicle.service';
import { NavbarComponent } from '../navbar/navbar.component';
import { Vehicle } from '../../models/vehicle.models';

@Component({
  selector: 'app-deregister-vehicle',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, SelectModule, ToastModule, NavbarComponent],
  providers: [MessageService],
  templateUrl: './deregister-vehicle.component.html'
})
export class DeregisterVehicleComponent implements OnInit {
  private authService = inject(AuthService);
  private vehicleService = inject(VehicleService);
  private messageService = inject(MessageService);
  private router = inject(Router);

  vehicles = signal<Vehicle[]>([]);
  selectedVehicleId = signal<number | null>(null);
  isLoading = signal(false);
  isSubmitting = signal(false);
  confirmed = signal(false);
  error = signal('');

  get selectedVehicle(): Vehicle | undefined {
    return this.vehicles().find(v => v.id === this.selectedVehicleId());
  }

  get eligibleVehicles(): Vehicle[] {
    return this.vehicles().filter(v => v.status !== 'Deregistered');
  }

  ngOnInit() {
    const user = this.authService.getUserData();
    if (!user) { this.router.navigate(['/login']); return; }

    this.isLoading.set(true);
    this.vehicleService.getVehiclesByOwnerId(user.id.toString()).subscribe({
      next: (res) => { this.vehicles.set(res.data); this.isLoading.set(false); },
      error: () => { this.error.set('Грешка при учитавању возила.'); this.isLoading.set(false); }
    });
  }

  onVehicleSelect(id: number) {
    this.selectedVehicleId.set(id);
    this.confirmed.set(false);
    this.error.set('');
  }

  submit() {
    const vehicle = this.selectedVehicle;
    if (!vehicle) return;
    if (!this.confirmed()) { this.error.set('Потврдите одјаву пре подношења захтева.'); return; }

    this.isSubmitting.set(true);
    this.error.set('');

    this.vehicleService.deleteVehicle(vehicle.id).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.messageService.add({ severity: 'success', summary: 'Одјава успешна', detail: `Возило ${vehicle.registrationNumber} је одјављено.`, life: 3000 });
        setTimeout(() => this.router.navigate(['/profile']), 2000);
      },
      error: () => {
        this.error.set('Одјава није успела. Покушајте поново.');
        this.isSubmitting.set(false);
      }
    });
  }

  goBack() { this.router.navigate(['/']); }
}
