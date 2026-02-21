import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../navbar/navbar.component';

@Component({
  selector: 'app-saobracajna-policija',
  standalone: true,
  imports: [CommonModule, NavbarComponent],
  templateUrl: './saobracajna-policija.component.html'
})
export class SaobracajnaPolicija {
  private router = inject(Router);
  goBack() { this.router.navigate(['/']); }
}
