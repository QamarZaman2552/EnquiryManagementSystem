import { Routes } from '@angular/router';
import { NewEnquiry } from './pages/new-enquiry/new-enquiry';
import { LoginPage } from './pages/login-page/login-page';
import { Enquires } from './pages/enquires/enquires';
import { Services } from './pages/services/services';

export const routes: Routes = [
  { path: '', redirectTo: 'enquiry', pathMatch: 'full' },
  { path: 'enquiry', component: NewEnquiry },
  { path: 'login', component: LoginPage },
  { path: 'admin/enquiries', component: Enquires },
  { path: 'admin/services', component: Services },
  { path: '**', redirectTo: 'enquiry' }
];
