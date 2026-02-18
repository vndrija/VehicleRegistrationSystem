import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

import { CardModule } from 'primeng/card';

import { AuthService } from '../../services/auth.service';
import { NavbarComponent } from '../navbar/navbar.component';

interface VehicleService {
  id: number;
  title: string;
  route: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, CardModule, NavbarComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  private router = inject(Router);
  private authService = inject(AuthService);

  services: VehicleService[] = [
    { id: 1, title: 'Упис у јединствени регистар возила', route: '/services/register-vehicle' },
    { id: 2, title: 'Издавање регистрационе налепнице за возило уписано у јединствени регистар (продужење регистрације)', route: '/services/renew-registration' },
    { id: 3, title: 'Привремена регистрација', route: '/services/temporary-registration' },
    { id: 4, title: 'Промена регистарских таблица', route: '/services/change-plates' },
    { id: 5, title: 'Промена саобраћајне дозволе', route: '/services/change-license' },
    { id: 6, title: 'Утискивање ИД ознака', route: '/services/id-marking' },
    { id: 7, title: 'Промена регистрационе налепнице', route: '/services/change-sticker' },
    { id: 8, title: 'Одјава возила', route: '/services/deregister-vehicle' },
    { id: 9, title: 'Издавање таблица за привремено означавање возила (ПРОБА)', route: '/services/test-plates' },
    { id: 10, title: 'Издавање саобраћајне дозволе по хитном поступку', route: '/services/urgent-license' },
    { id: 11, title: 'Заказивање термина за подношење захтева за регистрацију возила', route: '/services/schedule-appointment' },
    { id: 12, title: 'Упутство за постављање регистрационе налепнице за унутрашње лепљење', route: '/services/sticker-installation' },
    { id: 13, title: 'Упутство за скидање регистрационе налепнице за унутрашње лепљење', route: '/services/sticker-removal' },
    { id: 14, title: 'Читач електронске саобраћајне дозволе', route: '/services/license-reader' },
    { id: 15, title: 'Издавање овлашћења правном лицу за издавање таблица за привремено означавање возила (ПРОБА) и потврда о њиховом коришћењу', route: '/services/test-plates-authorization' },
    { id: 16, title: 'Издавање овлашћења правном лицу за издавање регистрационих налепница', route: '/services/sticker-authorization' },
    { id: 17, title: 'Замена правника овлашћеног за издавање регистрационих налепница у овлашћеном правном лицу', route: '/services/replace-authorized-lawyer' },
    { id: 18, title: 'ТЕХНИЧКИ ПРЕГЛЕД ВОЗИЛА', route: '/services/technical-inspection' },
    { id: 19, title: 'Списак овлашћених привредних друштава за издавање таблица за привремено означавање возила "Проба"', route: '/services/authorized-companies' }
  ];

  onServiceClick(service: VehicleService): void {
    if (!this.authService.isAuthenticated()) {
      this.authService.setReturnUrl(service.route);
      this.router.navigate(['/login']);
    } else {
      this.router.navigate([service.route]);
    }
  }

  isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }
}
