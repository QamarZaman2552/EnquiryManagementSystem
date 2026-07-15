import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Api } from '../../services/api';
import { AuthService } from '../../services/auth';
import { ToastService } from '../../services/toast.service';
import { Enquiry, Service } from '../../models/interfaces';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  private destroyRef = inject(DestroyRef);

  enquiriesList: Enquiry[] = [];
  servicesList: Service[] = [];
  isLoading = false;

  get adminUsername(): string { return this.auth.getUsername() || 'Admin'; }
  get adminInitial(): string  { return this.adminUsername.charAt(0).toUpperCase(); }

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
    let loaded = 0;
    const checkDone = () => { loaded++; if (loaded === 2) this.isLoading = false; };

    const sub1 = this.api.getEnquiresWithNames().subscribe({
      next: (enquiries) => { this.enquiriesList = enquiries; checkDone(); },
      error: () => { this.toast.error('Failed to load enquiries.'); checkDone(); }
    });

    const sub2 = this.api.getServices().subscribe({
      next: (services) => { this.servicesList = services; checkDone(); },
      error: () => { this.toast.error('Failed to load services.'); checkDone(); }
    });

    this.destroyRef.onDestroy(() => { sub1.unsubscribe(); sub2.unsubscribe(); });
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

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
