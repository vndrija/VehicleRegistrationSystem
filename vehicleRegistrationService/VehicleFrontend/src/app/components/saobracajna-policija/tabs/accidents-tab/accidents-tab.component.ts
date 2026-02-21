import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TrafficPoliceService, Accident } from '../../../../services/traffic-police.service';

@Component({
  selector: 'app-accidents-tab',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule],
  templateUrl: './accidents-tab.component.html'
})
export class AccidentsTabComponent {
  private policeService = inject(TrafficPoliceService);

  accidentsPlate = '';
  accidents = signal<Accident[]>([]);
  isAccidentsLoading = signal(false);
  accidentsSearched = signal(false);
  accidentsError = signal('');

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

  formatDate(dateStr: string): string {
    if (!dateStr) return '—';
    const d = new Date(dateStr);
    if (isNaN(d.getTime())) return '—';
    return d.toLocaleDateString('sr-RS', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  severityLabel(s: string): string {
    const m: Record<string, string> = {
      'MINOR': 'Лака', 'MAJOR': 'Тешка', 'CRITICAL': 'Критична', 'FATAL': 'Смртоносна'
    };
    return m[s] ?? s;
  }
}
