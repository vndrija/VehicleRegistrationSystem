import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'login',
    loadComponent: () => import('./components/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./components/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'profile',
    canActivate: [authGuard],
    loadComponent: () => import('./components/profile/profile.component').then(m => m.ProfileComponent)
  },
  {
    path: 'services/register-vehicle',
    canActivate: [authGuard],
    loadComponent: () => import('./components/register-vehicle/register-vehicle').then(m => m.RegisterVehicle)
  },
  {
    path: 'admin/registration-requests',
    canActivate: [authGuard],
    loadComponent: () => import('./components/admin-registration-requests/admin-registration-requests').then(m => m.AdminRegistrationRequests)
  },
  {
    path: 'services/:serviceName',
    canActivate: [authGuard],
    loadComponent: () => import('./components/home/home.component').then(m => m.HomeComponent)
  }
];
