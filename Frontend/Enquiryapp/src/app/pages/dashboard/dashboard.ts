import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Api } from '../../services/api';
import { AuthService } from '../../services/auth';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  enquiriesList: any[] = [];
  servicesList: any[] = [];
  isLoading = false;

  constructor(
    private api: Api,
    private auth: AuthService,
    private router: Router,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.api.getEnquiresWithNames().subscribe({
      next: (enquiries) => {
        this.enquiriesList = enquiries;
        this.api.getServices().subscribe({
          next: (services) => {
            this.servicesList = services;
            this.isLoading = false;
          },
          error: () => {
            this.isLoading = false;
            this.toast.error('Failed to load services.');
          }
        });
      },
      error: () => {
        this.isLoading = false;
        this.toast.error('Failed to load enquiries.');
      }
    });
  }

  get totalEnquiries(): number {
    return this.enquiriesList.length;
  }

  get pendingCount(): number {
    return this.enquiriesList.filter(e => !e.status || e.status === 'Pending').length;
  }

  get inProgressCount(): number {
    return this.enquiriesList.filter(e => e.status === 'In Progress').length;
  }

  get resolvedCount(): number {
    return this.enquiriesList.filter(e => e.status === 'Resolved').length;
  }

  get closedCount(): number {
    return this.enquiriesList.filter(e => e.status === 'Closed').length;
  }

  get totalServices(): number {
    return this.servicesList.length;
  }

  get recentEnquiries(): any[] {
    // Sort by date descending and take top 5
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

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
