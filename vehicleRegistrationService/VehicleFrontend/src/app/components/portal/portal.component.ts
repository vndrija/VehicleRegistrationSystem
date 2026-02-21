import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../navbar/navbar.component';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-portal',
  standalone: true,
  imports: [CommonModule, NavbarComponent],
  templateUrl: './portal.component.html'
})
export class PortalComponent {
  private router = inject(Router);
  private authService = inject(AuthService);

  isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  navigate(route: string) {
    if (!this.authService.isAuthenticated()) {
      this.authService.setReturnUrl(route);
      this.router.navigate(['/login']);
    } else {
      this.router.navigate([route]);
    }
  }
}
