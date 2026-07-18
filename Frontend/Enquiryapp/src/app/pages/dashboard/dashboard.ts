import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { Api } from '../../services/api';
import { AuthService } from '../../services/auth';
import { ToastService } from '../../services/toast.service';
import { AdminLayout } from '../admin-layout/admin-layout';
import { Enquiry, Service, ContactMessage } from '../../models/interfaces';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, AdminLayout],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  private destroyRef = inject(DestroyRef);
  private cdr = inject(ChangeDetectorRef);

  enquiriesList: Enquiry[] = [];
  servicesList: Service[] = [];
  contactMessages: ContactMessage[] = [];
  isLoading = false;

  constructor(
    private api: Api,
    private auth: AuthService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    const sub = forkJoin({
      enquiries: this.api.getEnquiries(1, 100),
      services: this.api.getServices(),
      contactMessages: this.api.getContactMessages()
    }).pipe(
      finalize(() => {
        this.isLoading = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: ({ enquiries, services, contactMessages }) => {
        this.enquiriesList = enquiries.data ?? [];
        this.servicesList = services ?? [];
        this.contactMessages = contactMessages ?? [];
        this.cdr.detectChanges();
      },
      error: () => {
        this.toast.error('Failed to load dashboard data. Please refresh the page.');
        this.cdr.detectChanges();
      }
    });
    this.destroyRef.onDestroy(() => sub.unsubscribe());
  }

  get totalEnquiries(): number { return this.enquiriesList.length; }
  get pendingCount(): number { return this.enquiriesList.filter(e => !e.status || e.status === 'Pending').length; }
  get inProgressCount(): number { return this.enquiriesList.filter(e => e.status === 'In Progress').length; }
  get resolvedCount(): number { return this.enquiriesList.filter(e => e.status === 'Resolved').length; }
  get closedCount(): number { return this.enquiriesList.filter(e => e.status === 'Closed').length; }
  get totalServices(): number { return this.servicesList.length; }

  get recentEnquiries(): Enquiry[] {
    return [...this.enquiriesList]
      .sort((a, b) => new Date(b.createdate).getTime() - new Date(a.createdate).getTime())
      .slice(0, 5);
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'In Progress': return 'badge bg-info text-dark';
      case 'Resolved': return 'badge bg-success';
      case 'Closed': return 'badge bg-secondary';
      default: return 'badge bg-warning text-dark';
    }
  }
}
