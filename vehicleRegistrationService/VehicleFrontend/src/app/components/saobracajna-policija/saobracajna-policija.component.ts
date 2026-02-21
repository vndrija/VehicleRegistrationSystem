import { Component, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, Validators } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';

import { NavbarComponent } from '../navbar/navbar.component';
import {
  TrafficPoliceService,
  StolenVehicle, VehicleDossier, Violation, Accident, Officer, VehicleFlag
} from '../../services/traffic-police.service';

export type Tab = 'check' | 'stolen' | 'violations' | 'accidents' | 'officers' | 'flags';

@Component({
  selector: 'app-saobracajna-policija',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, ButtonModule, ToastModule, NavbarComponent],
  providers: [MessageService],
  templateUrl: './saobracajna-policija.component.html'
})
export class SaobracajnaPolicija implements OnInit {
  private router = inject(Router);
  private policeService = inject(TrafficPoliceService);
  private messageService = inject(MessageService);
  private fb = inject(FormBuilder);

  // ── Tab navigation ──────────────────────────────────────────────
  activeTab = signal<Tab>('check');

  // ── Vehicle Status Check ─────────────────────────────────────────
  checkPlate = '';
  dossier = signal<VehicleDossier | null>(null);
  isDossierLoading = signal(false);
  dossierError = signal('');
  dossierSearched = signal(false);

  // ── Stolen Vehicles ──────────────────────────────────────────────
  stolenVehicles = signal<StolenVehicle[]>([]);
  isStolenLoading = signal(false);
  showReportStolenForm = signal(false);
  isReportingStolenLoading = signal(false);
  reportStolenError = signal('');
  reportStolenSuccess = signal('');

  reportStolenForm = this.fb.group({
    vehiclePlate: ['', [Validators.required, Validators.minLength(3)]],
    description:  ['', [Validators.required, Validators.minLength(10)]],
    contactInfo:  ['', [Validators.required, Validators.minLength(5)]]
  });

  // ── Violations ───────────────────────────────────────────────────
  violationsPlate = '';
  violations = signal<Violation[]>([]);
  isViolationsLoading = signal(false);
  violationsSearched = signal(false);
  violationsError = signal('');
  payingId = signal<number | null>(null);
  downloadingId = signal<number | null>(null);

  showIssueViolationForm = signal(false);
  isIssuingViolation = signal(false);
  issueViolationError = signal('');
  issueViolationSuccess = signal('');
  issueViolationForm = this.fb.group({
    vehiclePlate: ['', [Validators.required, Validators.minLength(3)]],
    type: ['SPEEDING', Validators.required],
    description: ['', [Validators.required, Validators.minLength(5)]],
    location: ['', Validators.required],
    fineAmount: [0, [Validators.required, Validators.min(1)]],
    offenderEmail: ['', [Validators.email]]
  });

  // ── Accidents ────────────────────────────────────────────────────
  accidentsPlate = '';
  accidents = signal<Accident[]>([]);
  isAccidentsLoading = signal(false);
  accidentsSearched = signal(false);
  accidentsError = signal('');
  showReportAccidentForm = signal(false);
  isReportingAccident = signal(false);
  reportAccidentError = signal('');
  reportAccidentSuccess = signal('');

  reportAccidentForm = this.fb.group({
    location:       ['', Validators.required],
    description:    ['', [Validators.required, Validators.minLength(10)]],
    severity:       ['MINOR', Validators.required],
    involvedPlates: ['', Validators.required]
  });

  // ── Officers ─────────────────────────────────────────────────────
  officers = signal<Officer[]>([]);
  isOfficersLoading = signal(false);

  showAddOfficerForm = signal(false);
  isAddingOfficer = signal(false);
  addOfficerError = signal('');
  addOfficerSuccess = signal('');
  addOfficerForm = this.fb.group({
    badgeNumber: ['', [Validators.required, Validators.minLength(3)]],
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    rank: ['Полицајац', Validators.required],
    stationId: ['', Validators.required],
    userId: ['']
  });

  // ── Flags ────────────────────────────────────────────────────────
  flagsPlate = '';
  flags = signal<VehicleFlag[]>([]);
  isFlagsLoading = signal(false);
  flagsSearched = signal(false);
  flagsError = signal('');
  resolvingFlagId = signal<number | null>(null);

  showAddFlagForm = signal(false);
  isAddingFlag = signal(false);
  addFlagError = signal('');
  addFlagSuccess = signal('');
  addFlagForm = this.fb.group({
    vehiclePlate: ['', [Validators.required, Validators.minLength(3)]],
    flagType: ['', Validators.required],
    description: ['', [Validators.required, Validators.minLength(5)]]
  });

  // ────────────────────────────────────────────────────────────────
  ngOnInit() {
  // DEBUG CHECK
  const t = localStorage.getItem('token');
  console.log('Component Init - Token in storage:', t ? t.substring(0, 10) + '...' : 'NULL');

  this.loadStolenVehicles();
  this.loadOfficers();
}


  setTab(tab: Tab) { this.activeTab.set(tab); }

  // ── Vehicle Status ───────────────────────────────────────────────
  checkVehicleStatus() {
    const plate = this.checkPlate.trim();
    if (!plate) return;
    this.isDossierLoading.set(true);
    this.dossierError.set('');
    this.dossier.set(null);
    this.dossierSearched.set(true);
    this.policeService.getVehicleStatus(plate).subscribe({
      next: (data) => { this.dossier.set(data); this.isDossierLoading.set(false); },
      error: () => { this.dossierError.set('Грешка при провери. Проверите регистарску ознаку.'); this.isDossierLoading.set(false); }
    });
  }

  // ── Stolen ───────────────────────────────────────────────────────
  loadStolenVehicles() {
    this.isStolenLoading.set(true);
    this.policeService.getStolenVehicles().subscribe({
      next: (data) => { this.stolenVehicles.set(data); this.isStolenLoading.set(false); },
      error: () => { this.isStolenLoading.set(false); }
    });
  }

  toggleReportStolenForm() {
    this.showReportStolenForm.update(v => !v);
    this.reportStolenError.set('');
    this.reportStolenSuccess.set('');
    if (!this.showReportStolenForm()) this.reportStolenForm.reset();
  }

  reportStolen() {
    if (this.reportStolenForm.invalid) { this.reportStolenForm.markAllAsTouched(); return; }
    this.isReportingStolenLoading.set(true);
    this.reportStolenError.set('');
    this.policeService.reportStolenVehicle(this.reportStolenForm.value as any).subscribe({
      next: () => {
        this.isReportingStolenLoading.set(false);
        this.reportStolenSuccess.set('Пријава је успешно поднета. Возило је додато у евиденцију.');
        this.reportStolenForm.reset();
        this.loadStolenVehicles();
        setTimeout(() => { this.showReportStolenForm.set(false); this.reportStolenSuccess.set(''); }, 3000);
      },
      error: () => { this.reportStolenError.set('Грешка при пријави. Покушајте поново.'); this.isReportingStolenLoading.set(false); }
    });
  }

  // ── Violations ───────────────────────────────────────────────────
  searchViolations() {
    const plate = this.violationsPlate.trim();
    if (!plate) return;
    this.isViolationsLoading.set(true);
    this.violationsError.set('');
    this.violationsSearched.set(true);
    this.policeService.getViolationsByPlate(plate).subscribe({
      next: (data) => { this.violations.set(data); this.isViolationsLoading.set(false); },
      error: () => { this.violationsError.set('Грешка при претрази.'); this.isViolationsLoading.set(false); }
    });
  }

  toggleIssueViolationForm() {
    this.showIssueViolationForm.update(v => !v);
    this.issueViolationError.set('');
    this.issueViolationSuccess.set('');
    if (!this.showIssueViolationForm()) this.issueViolationForm.reset({ type: 'SPEEDING', fineAmount: 0 });
  }

  issueViolation() {
    if (this.issueViolationForm.invalid) { this.issueViolationForm.markAllAsTouched(); return; }
    this.isIssuingViolation.set(true);
    this.issueViolationError.set('');
    
    // Default officerId for now
    const dto = { ...this.issueViolationForm.value, officerId: 1 } as any;

    this.policeService.issueViolation(dto).subscribe({
      next: () => {
        this.isIssuingViolation.set(false);
        this.issueViolationSuccess.set('Прекршај је успешно евидентиран.');
        this.issueViolationForm.reset({ type: 'SPEEDING', fineAmount: 0 });
        if (this.violationsPlate === dto.vehiclePlate) {
          this.searchViolations();
        }
        setTimeout(() => { this.showIssueViolationForm.set(false); this.issueViolationSuccess.set(''); }, 3000);
      },
      error: () => { this.issueViolationError.set('Грешка при евидентирању прекршаја.'); this.isIssuingViolation.set(false); }
    });
  }

  payViolation(id: number) {
    this.payingId.set(id);
    this.policeService.payViolation(id).subscribe({
      next: () => {
        this.payingId.set(null);
        this.messageService.add({ severity: 'success', summary: 'Успешно', detail: 'Казна је плаћена.', life: 3000 });
        this.searchViolations();
      },
      error: () => {
        this.payingId.set(null);
        this.messageService.add({ severity: 'error', summary: 'Грешка', detail: 'Плаћање није успело.', life: 3000 });
      }
    });
  }

  downloadPdf(id: number) {
    this.downloadingId.set(id);
    this.policeService.downloadViolationPdf(id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `prekrsaj_${id}.pdf`;
        a.click();
        URL.revokeObjectURL(url);
        this.downloadingId.set(null);
      },
      error: () => {
        this.downloadingId.set(null);
        this.messageService.add({ severity: 'error', summary: 'Грешка', detail: 'Преузимање PDF-а није успело.', life: 3000 });
      }
    });
  }

  // ── Accidents ────────────────────────────────────────────────────
  searchAccidents() {
    const plate = this.accidentsPlate.trim();
    if (!plate) return;
    this.isAccidentsLoading.set(true);
    this.accidentsError.set('');
    this.accidentsSearched.set(true);
    this.policeService.getAccidentsByPlate(plate).subscribe({
      next: (data) => { this.accidents.set(data); this.isAccidentsLoading.set(false); },
      error: () => { this.accidentsError.set('Грешка при претрази.'); this.isAccidentsLoading.set(false); }
    });
  }

  toggleReportAccidentForm() {
    this.showReportAccidentForm.update(v => !v);
    this.reportAccidentError.set('');
    this.reportAccidentSuccess.set('');
    if (!this.showReportAccidentForm()) this.reportAccidentForm.reset({ severity: 'MINOR' });
  }

  reportAccident() {
    if (this.reportAccidentForm.invalid) { this.reportAccidentForm.markAllAsTouched(); return; }
    this.isReportingAccident.set(true);
    this.reportAccidentError.set('');
    this.policeService.reportAccident(this.reportAccidentForm.value as any).subscribe({
      next: () => {
        this.isReportingAccident.set(false);
        this.reportAccidentSuccess.set('Незгода је успешно пријављена.');
        this.reportAccidentForm.reset({ severity: 'MINOR' });
        setTimeout(() => { this.showReportAccidentForm.set(false); this.reportAccidentSuccess.set(''); }, 3000);
      },
      error: () => { this.reportAccidentError.set('Грешка при пријави незгоде.'); this.isReportingAccident.set(false); }
    });
  }

  // ── Officers ─────────────────────────────────────────────────────
  loadOfficers() {
    this.isOfficersLoading.set(true);
    this.policeService.getOfficers().subscribe({
      next: (data) => { this.officers.set(data); this.isOfficersLoading.set(false); },
      error: () => { this.isOfficersLoading.set(false); }
    });
  }

  toggleAddOfficerForm() {
    this.showAddOfficerForm.update(v => !v);
    this.addOfficerError.set('');
    this.addOfficerSuccess.set('');
    if (!this.showAddOfficerForm()) this.addOfficerForm.reset({ rank: 'Полицајац' });
  }

  addOfficer() {
    if (this.addOfficerForm.invalid) { this.addOfficerForm.markAllAsTouched(); return; }
    this.isAddingOfficer.set(true);
    this.addOfficerError.set('');
    this.policeService.createOfficer(this.addOfficerForm.value as any).subscribe({
      next: () => {
        this.isAddingOfficer.set(false);
        this.addOfficerSuccess.set('Службеник је успешно додат.');
        this.addOfficerForm.reset({ rank: 'Полицајац' });
        this.loadOfficers();
        setTimeout(() => { this.showAddOfficerForm.set(false); this.addOfficerSuccess.set(''); }, 3000);
      },
      error: () => { this.addOfficerError.set('Грешка при додавању службеника.'); this.isAddingOfficer.set(false); }
    });
  }

  // ── Flags ────────────────────────────────────────────────────────
  searchFlags() {
    const plate = this.flagsPlate.trim();
    if (!plate) return;
    this.isFlagsLoading.set(true);
    this.flagsError.set('');
    this.flagsSearched.set(true);
    this.policeService.getFlagsByPlate(plate).subscribe({
      next: (data) => { this.flags.set(data); this.isFlagsLoading.set(false); },
      error: () => { this.flagsError.set('Грешка при претрази.'); this.isFlagsLoading.set(false); }
    });
  }

  toggleAddFlagForm() {
    this.showAddFlagForm.update(v => !v);
    this.addFlagError.set('');
    this.addFlagSuccess.set('');
    if (!this.showAddFlagForm()) this.addFlagForm.reset();
  }

  addFlag() {
    if (this.addFlagForm.invalid) { this.addFlagForm.markAllAsTouched(); return; }
    this.isAddingFlag.set(true);
    this.addFlagError.set('');
    this.policeService.addFlag(this.addFlagForm.value as any).subscribe({
      next: () => {
        this.isAddingFlag.set(false);
        this.addFlagSuccess.set('Маркирање је успешно додато.');
        const plate = this.addFlagForm.get('vehiclePlate')?.value;
        this.addFlagForm.reset();
        if (this.flagsPlate === plate) {
          this.searchFlags();
        }
        setTimeout(() => { this.showAddFlagForm.set(false); this.addFlagSuccess.set(''); }, 3000);
      },
      error: () => { this.addFlagError.set('Грешка при додавању маркирања.'); this.isAddingFlag.set(false); }
    });
  }

  resolveFlag(id: number) {
    this.resolvingFlagId.set(id);
    this.policeService.resolveFlag(id).subscribe({
      next: () => {
        this.resolvingFlagId.set(null);
        this.messageService.add({ severity: 'success', summary: 'Успешно', detail: 'Маркирање је уклоњено.', life: 3000 });
        this.searchFlags();
      },
      error: () => {
        this.resolvingFlagId.set(null);
        this.messageService.add({ severity: 'error', summary: 'Грешка', detail: 'Уклањање није успело.', life: 3000 });
      }
    });
  }

  // ── Helpers ──────────────────────────────────────────────────────
  goBack() { this.router.navigate(['/']); }

  formatDate(dateStr: string): string {
    if (!dateStr) return '—';
    const d = new Date(dateStr);
    if (isNaN(d.getTime())) return '—';
    return d.toLocaleDateString('sr-RS', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  violationTypeLabel(type: string): string {
    const m: Record<string, string> = {
      'SPEEDING': 'Брзина', 'PARKING': 'Паркирање', 'DUI': 'Алкохол',
      'RED_LIGHT': 'Семафор', 'EXPIRED_DOCS': 'Документа', 'RECKLESS_DRIVING': 'Нехатна вожња'
    };
    return m[type] ?? type;
  }

  severityLabel(s: string): string {
    const m: Record<string, string> = {
      'MINOR': 'Лака', 'MAJOR': 'Тешка', 'CRITICAL': 'Критична', 'FATAL': 'Смртоносна'
    };
    return m[s] ?? s;
  }
}
