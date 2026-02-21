import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

import { AuthService } from '../../services/auth.service';
import { NavbarComponent } from '../navbar/navbar.component';

interface VehicleService {
  id: number;
  title: string;
  route: string;
  icon: string;
  comingSoon?: boolean;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, NavbarComponent, RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  private router = inject(Router);
  private authService = inject(AuthService);

  services: VehicleService[] = [
    { id: 1,  title: 'Упис у јединствени регистар возила',                                                                                                                       route: '/services/register-vehicle',          icon: 'pi-car' },
    { id: 2,  title: 'Издавање регистрационе налепнице за возило уписано у јединствени регистар (продужење регистрације)',                                                        route: '/services/renew-registration',        icon: 'pi-refresh' },
    { id: 3,  title: 'Привремена регистрација',                                                                                                                                   route: '/services/temporary-registration',    icon: 'pi-clock',             comingSoon: true },
    { id: 4,  title: 'Промена регистарских таблица',                                                                                                                              route: '/services/change-plates',             icon: 'pi-id-card' },
    { id: 5,  title: 'Промена саобраћајне дозволе',                                                                                                                               route: '/services/change-license',            icon: 'pi-pencil',            comingSoon: true },
    { id: 6,  title: 'Утискивање ИД ознака',                                                                                                                                      route: '/services/id-marking',                icon: 'pi-tag',               comingSoon: true },
    { id: 7,  title: 'Промена регистрационе налепнице',                                                                                                                           route: '/services/change-sticker',            icon: 'pi-file',              comingSoon: true },
    { id: 8,  title: 'Одјава возила',                                                                                                                                             route: '/services/deregister-vehicle',        icon: 'pi-times-circle' },
    { id: 9,  title: 'Издавање таблица за привремено означавање возила (ПРОБА)',                                                                                                  route: '/services/test-plates',               icon: 'pi-circle',            comingSoon: true },
    { id: 10, title: 'Издавање саобраћајне дозволе по хитном поступку',                                                                                                          route: '/services/urgent-license',            icon: 'pi-exclamation-triangle', comingSoon: true },
    { id: 11, title: 'Заказивање термина за подношење захтева за регистрацију возила',                                                                                            route: '/services/schedule-appointment',      icon: 'pi-calendar',          comingSoon: true },
    { id: 12, title: 'Упутство за постављање регистрационе налепнице за унутрашње лепљење',                                                                                     route: '/services/sticker-installation',      icon: 'pi-book',              comingSoon: true },
    { id: 13, title: 'Упутство за скидање регистрационе налепнице за унутрашње лепљење',                                                                                        route: '/services/sticker-removal',           icon: 'pi-book',              comingSoon: true },
    { id: 14, title: 'Читач електронске саобраћајне дозволе',                                                                                                                    route: '/services/license-reader',            icon: 'pi-mobile',            comingSoon: true },
    { id: 15, title: 'Издавање овлашћења правном лицу за издавање таблица за привремено означавање возила (ПРОБА) и потврда о њиховом коришћењу',                               route: '/services/test-plates-authorization', icon: 'pi-building',          comingSoon: true },
    { id: 16, title: 'Издавање овлашћења правном лицу за издавање регистрационих налепница',                                                                                     route: '/services/sticker-authorization',     icon: 'pi-building',          comingSoon: true },
    { id: 17, title: 'Замена правника овлашћеног за издавање регистрационих налепница у овлашћеном правном лицу',                                                                route: '/services/replace-authorized-lawyer', icon: 'pi-users',             comingSoon: true },
    { id: 18, title: 'ТЕХНИЧКИ ПРЕГЛЕД ВОЗИЛА',                                                                                                                                  route: '/services/technical-inspection',      icon: 'pi-cog',               comingSoon: true },
    { id: 19, title: 'Списак овлашћених привредних друштава за издавање таблица за привремено означавање возила "Проба"',                                                        route: '/services/authorized-companies',      icon: 'pi-list',              comingSoon: true },
  ];

  onServiceClick(service: VehicleService): void {
    if (service.comingSoon) return;
    if (!this.authService.isAuthenticated()) {
      this.authService.setReturnUrl('/mup-vozila');
      this.router.navigate(['/login']);
    } else {
      this.router.navigate([service.route]);
    }
  }

  isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }
}
