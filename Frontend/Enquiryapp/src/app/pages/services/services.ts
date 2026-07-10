import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Api } from '../../services/api';
import { AuthService } from '../../services/auth';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-services',
  imports: [CommonModule, FormsModule],
  templateUrl: './services.html',
  styleUrl: './services.css',
})
export class Services implements OnInit {
  servicesList: any[] = [];
  isLoading = false;
  isSaving = false;

  newService = { serviceName: '', rate: 0, isActive: true };
  showEditModal = false;
  editService: any = { serviceId: 0, serviceName: '', rate: 0, isActive: true };

  get totalCount():  number { return this.servicesList.length; }
  get activeCount(): number { return this.servicesList.filter(s => s.isActive).length; }

  constructor(private api: Api, private auth: AuthService, private router: Router, private toast: ToastService) {}

  ngOnInit() { this.loadService(); }

  loadService() {
    this.isLoading = true;
    this.api.getServices().subscribe({
      next: (data) => { this.servicesList = data; this.isLoading = false; },
      error: () => { this.isLoading = false; this.toast.error('Failed to load services.'); }
    });
  }

  addServices() {
    this.isSaving = true;
    this.api.addNewServices(this.newService).subscribe({
      next: () => {
        this.isSaving = false;
        this.toast.success('Service added successfully!');
        this.loadService();
        this.newService = { serviceName: '', rate: 0, isActive: true };
      },
      error: () => { this.isSaving = false; this.toast.error('Failed to add service.'); }
    });
  }

  openEditModal(s: any)  { this.editService = { ...s }; this.showEditModal = true; }
  closeEditModal()        { this.showEditModal = false; }

  updateService() {
    this.isSaving = true;
    this.api.updateService(this.editService.serviceId, this.editService).subscribe({
      next: () => {
        this.isSaving = false;
        this.toast.success('Service updated successfully!');
        this.showEditModal = false;
        this.loadService();
      },
      error: () => { this.isSaving = false; this.toast.error('Failed to update service.'); }
    });
  }

  toggleStatus(s: any) {
    const updated = { ...s, isActive: !s.isActive };
    this.api.updateService(s.serviceId, updated).subscribe({
      next: () => { s.isActive = !s.isActive; this.toast.success(`Service ${s.isActive ? 'activated' : 'deactivated'}.`); },
      error: () => this.toast.error('Failed to update status.')
    });
  }

  deleteService(id: number) {
    if (!confirm('Are you sure you want to delete this service?')) return;
    this.api.deleteService(id).subscribe({
      next: () => { this.toast.success('Service deleted.'); this.loadService(); },
      error: () => this.toast.error('Failed to delete service.')
    });
  }

  logout() { this.auth.logout(); this.router.navigate(['/login']); }
}
