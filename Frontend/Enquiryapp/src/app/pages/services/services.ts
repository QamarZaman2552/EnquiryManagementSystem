import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../../services/api';
import { AuthService } from '../../services/auth';
import { ToastService } from '../../services/toast.service';
import { AdminLayout } from '../admin-layout/admin-layout';
import { Service } from '../../models/interfaces';

@Component({
  selector: 'app-services',
  imports: [CommonModule, FormsModule, AdminLayout],
  templateUrl: './services.html',
  styleUrl: './services.css',
})
export class Services implements OnInit {
  private destroyRef = inject(DestroyRef);

  servicesList: Service[] = [];
  isLoading = false;
  isSaving = false;

  newService = { serviceName: '', rate: 0, isActive: true };
  showEditModal = false;
  editService: Partial<Service> & { serviceId: number } = { serviceId: 0, serviceName: '', rate: 0, isActive: true };

  get totalCount():  number { return this.servicesList.length; }
  get activeCount(): number { return this.servicesList.filter(s => s.isActive).length; }

  constructor(private api: Api, private auth: AuthService, private toast: ToastService) {}

  ngOnInit() { this.loadService(); }

  loadService() {
    this.isLoading = true;
    const sub = this.api.getServices().subscribe({
      next: (data) => { this.servicesList = data; this.isLoading = false; },
      error: () => { this.isLoading = false; this.toast.error('Failed to load services.'); }
    });
    this.destroyRef.onDestroy(() => sub.unsubscribe());
  }

  addServices() {
    this.isSaving = true;
    const sub = this.api.addNewServices(this.newService).subscribe({
      next: () => {
        this.isSaving = false;
        this.toast.success('Service added successfully!');
        this.loadService();
        this.newService = { serviceName: '', rate: 0, isActive: true };
      },
      error: () => { this.isSaving = false; this.toast.error('Failed to add service.'); }
    });
    this.destroyRef.onDestroy(() => sub.unsubscribe());
  }

  openEditModal(s: Service)  { this.editService = { ...s }; this.showEditModal = true; }
  closeEditModal()        { this.showEditModal = false; }

  updateService() {
    this.isSaving = true;
    const sub = this.api.updateService(this.editService.serviceId, this.editService).subscribe({
      next: () => {
        this.isSaving = false;
        this.toast.success('Service updated successfully!');
        this.showEditModal = false;
        this.loadService();
      },
      error: () => { this.isSaving = false; this.toast.error('Failed to update service.'); }
    });
    this.destroyRef.onDestroy(() => sub.unsubscribe());
  }

  toggleStatus(s: Service) {
    const updated = { ...s, isActive: !s.isActive };
    const sub = this.api.updateService(s.serviceId, updated).subscribe({
      next: () => { s.isActive = !s.isActive; this.toast.success(`Service ${s.isActive ? 'activated' : 'deactivated'}.`); },
      error: () => this.toast.error('Failed to update status.')
    });
    this.destroyRef.onDestroy(() => sub.unsubscribe());
  }

  deleteService(id: number) {
    if (!confirm('Are you sure you want to delete this service?')) return;
    const sub = this.api.deleteService(id).subscribe({
      next: () => { this.toast.success('Service deleted.'); this.loadService(); },
      error: () => this.toast.error('Failed to delete service.')
    });
    this.destroyRef.onDestroy(() => sub.unsubscribe());
  }
}
