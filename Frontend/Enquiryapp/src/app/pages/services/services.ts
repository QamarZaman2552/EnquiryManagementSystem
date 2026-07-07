import { CommonModule } from '@angular/common';
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Api } from '../../services/api';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-services',
  imports: [CommonModule, FormsModule],
  templateUrl: './services.html',
  styleUrl: './services.css',
})
export class Services implements OnInit {
  servicesList: any[] = [];

  newService = {
    serviceName: '',
    rate: 0,
    isActive: true
  };

  // Edit modal state
  showEditModal = false;
  editService: any = { serviceId: 0, serviceName: '', rate: 0, isActive: true };

  get totalCount(): number {
    return this.servicesList.length;
  }

  get activeCount(): number {
    return this.servicesList.filter(s => s.isActive).length;
  }

  constructor(
    private api: Api,
    private cdr: ChangeDetectorRef,
    private auth: AuthService,
    private router: Router
  ) { }

  ngOnInit() {
    this.loadService();
  }

  loadService() {
    this.api.getServices().subscribe({
      next: (data) => {
        this.servicesList = data;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.log('Error loading services', err);
      }
    });
  }

  addServices() {
    this.api.addNewServices(this.newService).subscribe({
      next: () => {
        alert('Service added successfully');
        this.loadService();
        this.newService = { serviceName: '', rate: 0, isActive: true };
      },
      error: () => {
        alert('Error adding service');
      }
    });
  }

  openEditModal(s: any) {
    this.editService = { ...s };
    this.showEditModal = true;
  }

  closeEditModal() {
    this.showEditModal = false;
  }

  updateService() {
    this.api.updateService(this.editService.serviceId, this.editService).subscribe({
      next: () => {
        alert('Service updated successfully');
        this.showEditModal = false;
        this.loadService();
      },
      error: () => {
        alert('Error updating service');
      }
    });
  }

  toggleStatus(s: any) {
    const updated = { ...s, isActive: !s.isActive };
    this.api.updateService(s.serviceId, updated).subscribe({
      next: () => {
        s.isActive = !s.isActive;
        this.cdr.detectChanges();
      },
      error: () => {
        alert('Error updating status');
      }
    });
  }

  deleteService(id: number) {
    if (!confirm('Are you sure you want to delete this service?')) return;
    this.api.deleteService(id).subscribe({
      next: () => {
        alert('Service deleted successfully');
        this.loadService();
      },
      error: () => {
        alert('Error deleting service');
      }
    });
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
