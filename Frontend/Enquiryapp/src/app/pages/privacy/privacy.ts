import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../../services/api';
import { ToastService } from '../../services/toast.service';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-privacy',
  imports: [CommonModule, FormsModule],
  templateUrl: './privacy.html',
  styleUrl: './privacy.css',
})
export class Privacy {
  currentYear = new Date().getFullYear();

  email = '';
  exportResult: any = null;
  isLoading = false;

  constructor(private api: Api, private toast: ToastService, public auth: AuthService) {}

  get isAdmin(): boolean {
    return this.auth.isAdmin();
  }

  scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  requestExport() {
    if (!this.email) return;
    this.isLoading = true;
    this.exportResult = null;
    this.api.requestDataExport(this.email).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.exportResult = res;
        this.toast.success('Data export requested. Results shown below.');
      },
      error: () => {
        this.isLoading = false;
        this.toast.error('Failed to request data export.');
      }
    });
  }

  requestDeletion() {
    if (!this.email || !confirm(`Are you sure you want to request deletion of all data associated with ${this.email}? This action cannot be undone.`)) return;
    this.isLoading = true;
    this.api.requestDataDeletion(this.email).subscribe({
      next: () => {
        this.isLoading = false;
        this.exportResult = null;
        this.toast.success('Data deletion request processed.');
      },
      error: () => {
        this.isLoading = false;
        this.toast.error('Failed to process data deletion request.');
      }
    });
  }
}
