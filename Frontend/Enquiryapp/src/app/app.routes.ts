import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'enquiry', pathMatch: 'full' },
  {
    path: 'enquiry',
    loadComponent: () => import('./pages/new-enquiry/new-enquiry').then(m => m.NewEnquiry)
  },
  {
    path: 'contact',
    loadComponent: () => import('./pages/contact/contact').then(m => m.Contact)
  },
  {
    path: 'login',
    loadComponent: () => import('./pages/login-page/login-page').then(m => m.LoginPage)
  },
  {
    path: 'admin/dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/dashboard/dashboard').then(m => m.Dashboard)
  },
  {
    path: 'admin/enquiries',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/enquires/enquires').then(m => m.Enquires)
  },
  {
    path: 'admin/services',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/services/services').then(m => m.Services)
  },
  { path: '**', redirectTo: 'enquiry' }
];
