import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { TableModule } from 'primeng/table';
import { ProgressSpinnerModule  } from 'primeng/progressspinner';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';

import { PoliceService, PoliceVehicleReport, TrafficViolation } from '../../services/police.service';

@Component({
  selector: 'app-vehicle-police-status',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    MessageModule,
    TableModule,
    ProgressSpinnerModule,
    ToastModule
  ],
  providers: [MessageService],
  template: `
    <p-toast></p-toast>

    <div class="vehicle-police-status">
      <p-card class="police-card">
        <ng-template pTemplate="header">
          <div class="police-header">
            <div class="header-title">
              <i class="pi pi-shield text-2xl text-blue-600 mr-3"></i>
              <h3 class="text-xl font-bold">Policijska Verifikacija</h3>
            </div>
          </div>
        </ng-template>

        @if (isLoading()) {
          <div class="text-center py-8">
            <i class="pi pi-spin pi-spinner text-3xl text-blue-600"></i>
            <p class="mt-2 text-gray-600">Proveravamo status vozila sa policijom...</p>
          </div>
        } @else if (policeReport()) {
          <div class="space-y-4">
            <!-- Status Badge -->
            <div class="status-section">
              <div class="flex items-center justify-between mb-4">
                <h4 class="font-semibold text-gray-800">Status Vozila</h4>
                <span [ngClass]="getStatusBadgeClass()">
                  {{ getStatusInSerbian(policeReport()!.status) }}
                </span>
              </div>
            </div>

            <!-- Wanted Status -->
            @if (policeReport()!.isWanted) {
              <p-message
                severity="error"
                [text]="'⚠️ TRAŽENO: ' + (policeReport()!.wantedReason || 'Nepoznat razlog')"
                styleClass="w-full"
              ></p-message>
            }

            <!-- Outstanding Fines -->
            <div class="fines-section">
              <div class="flex items-center justify-between p-4 bg-yellow-50 rounded-lg border border-yellow-200">
                <div>
                  <p class="text-sm text-gray-600">Neplaćene Kazne</p>
                  <p class="text-2xl font-bold text-yellow-700">RSD{{ policeReport()!.outstandingFines }}</p>
                </div>
                <i class="pi pi-exclamation-circle text-3xl text-yellow-600"></i>
              </div>
            </div>

            <!-- Traffic Violations -->
            @if (policeReport()!.violationCount > 0) {
              <div class="violations-section">
                <h4 class="font-semibold text-gray-800 mb-3">
                  Prometne Povrede ({{ policeReport()!.violationCount }})
                </h4>

                <p-table
                  [value]="policeReport()!.activeViolations"
                  [paginator]="true"
                  [rows]="5"
                  responsiveLayout="scroll"
                  styleClass="p-datatable-sm"
                >
                  <ng-template pTemplate="header">
                    <tr>
                      <th>Tip</th>
                      <th>Lokacija</th>
                      <th>Datum</th>
                      <th>Kazna</th>
                      <th>Status</th>
                    </tr>
                  </ng-template>
                  <ng-template pTemplate="body" let-violation>
                    <tr>
                      <td>
                        <span class="font-semibold">{{ violation.violationType }}</span>
                      </td>
                      <td>{{ violation.location }}</td>
                      <td>{{ formatDate(violation.dateOfViolation) }}</td>
                      <td class="font-bold">RSD{{ violation.fineAmount }}</td>
                      <td>
                        <span [ngClass]="getViolationStatusClass(violation.status)">
                          {{ getViolationStatusInSerbian(violation.status) }}
                        </span>
                      </td>
                    </tr>
                  </ng-template>
                  <ng-template pTemplate="emptymessage">
                    <tr>
                      <td colspan="5" class="text-center text-gray-500">Nema povrede u evidenciji</td>
                    </tr>
                  </ng-template>
                </p-table>
              </div>
            } @else {
              <p-message
                severity="success"
                text="✓ Nema prometnih povreda u evidenciji"
                styleClass="w-full"
              ></p-message>
            }

            <!-- Clear Status -->
            @if (!policeReport()!.isWanted && policeReport()!.violationCount === 0) {
              <p-message
                severity="success"
                text="✓ Status vozila je ČIST"
                styleClass="w-full"
              ></p-message>
            }

            <!-- Last Checked -->
            <div class="text-xs text-gray-500 text-center pt-2">
              Poslednja proverka: {{ formatDate(policeReport()!.checkedAt) }}
            </div>
          </div>
        } @else if (error()) {
          <p-message
            severity="error"
            [text]="error()!"
            styleClass="w-full"
          ></p-message>
        }

        <!-- Action Button -->
        <ng-template pTemplate="footer">
          <div class="flex justify-end gap-2">
            <p-button
              label="Osveži Status"
              icon="pi pi-refresh"
              severity="secondary"
              [loading]="isLoading()"
              (onClick)="checkPoliceStatus()"
            ></p-button>
            <p-button
              label="Prijavi Policiji"
              icon="pi pi-send"
              severity="secondary"
              [loading]="isReporting()"
              (onClick)="reportVehicle()"
            ></p-button>
          </div>
        </ng-template>
      </p-card>
    </div>
  `,
  styles: [`
    .police-header {
      background: linear-gradient(135deg, #1e3a8a 0%, #1e40af 100%);
      color: white;
      padding: 1.5rem;
      border-radius: 0.5rem;
    }

    .header-title {
      display: flex;
      align-items: center;
    }

    .header-title h3 {
      margin: 0;
      color: white;
    }

    .police-card {
      border: 2px solid #dbeafe;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    }

    .status-section,
    .fines-section,
    .violations-section {
      padding: 0.5rem 0;
    }

    .vehicle-police-status {
      margin: 1rem 0;
    }
  `]
})
export class VehiclePoliceStatusComponent implements OnInit {
  @Input() vehicleId!: number;

  private policeService = inject(PoliceService);
  private messageService = inject(MessageService);

  policeReport = signal<any | null>(null);
  isLoading = signal(false);
  isReporting = signal(false);
  error = signal<string | null>(null);

  ngOnInit() {
    this.checkPoliceStatus();
  }

  checkPoliceStatus() {
    this.isLoading.set(true);
    this.error.set(null);

    this.policeService.checkVehicleWithPolice(this.vehicleId).subscribe({
      next: (response) => {
        this.policeReport.set(response.policeReport);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Neuspešna proverka statusa vozila sa policijom');
        this.isLoading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Greška',
          detail: 'Nije moguće dobiti podatke policije',
          life: 5000
        });
        console.error('Police check error:', err);
      }
    });
  }

  reportVehicle() {
    this.isReporting.set(true);

    this.policeService.reportVehicleToPolice(this.vehicleId).subscribe({
      next: (response) => {
        this.isReporting.set(false);
        this.messageService.add({
          severity: 'success',
          summary: 'Prijavljeno',
          detail: 'Vozilo je prijavljeno policiji',
          life: 3000
        });
      },
      error: (err) => {
        this.isReporting.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Greška',
          detail: 'Neuspešno prijavljivanje vozila policiji',
          life: 5000
        });
        console.error('Report error:', err);
      }
    });
  }

  getStatusInSerbian(status: string): string {
    switch (status) {
      case 'Wanted':
        return 'Traženo';
      case 'Alert':
        return 'Upozorenje';
      case 'Clear':
        return 'Čisto';
      default:
        return status;
    }
  }

  getStatusBadgeClass(): string {
    const status = this.policeReport()?.status;
    const baseClass = 'px-3 py-1 rounded-full text-sm font-semibold';

    switch (status) {
      case 'Wanted':
        return `${baseClass} bg-red-100 text-red-700 border border-red-300`;
      case 'Alert':
        return `${baseClass} bg-yellow-100 text-yellow-700 border border-yellow-300`;
      case 'Clear':
        return `${baseClass} bg-green-100 text-green-700 border border-green-300`;
      default:
        return `${baseClass} bg-gray-100 text-gray-700 border border-gray-300`;
    }
  }

  getViolationStatusInSerbian(status: string): string {
    switch (status?.toLowerCase()) {
      case 'pending':
        return 'Na čekanju';
      case 'paid':
        return 'Plaćeno';
      case 'disputed':
        return 'Sporeno';
      default:
        return status;
    }
  }

  getViolationStatusClass(status: string): string {
    const baseClass = 'px-2 py-1 rounded text-xs font-semibold';

    switch (status?.toLowerCase()) {
      case 'pending':
        return `${baseClass} bg-red-100 text-red-700`;
      case 'paid':
        return `${baseClass} bg-green-100 text-green-700`;
      case 'disputed':
        return `${baseClass} bg-yellow-100 text-yellow-700`;
      default:
        return `${baseClass} bg-gray-100 text-gray-700`;
    }
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }
}
