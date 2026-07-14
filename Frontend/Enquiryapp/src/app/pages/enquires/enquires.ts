import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Api } from '../../services/api';
import { AuthService } from '../../services/auth';
import { ToastService } from '../../services/toast.service';
import { Enquiry } from '../../models/interfaces';

@Component({
  selector: 'app-enquires',
  imports: [CommonModule, FormsModule],
  templateUrl: './enquires.html',
  styleUrl: './enquires.css',
})
export class Enquires implements OnInit {
  private destroyRef = inject(DestroyRef);

  enquiresList: Enquiry[] = [];
  selectedEnquiry: Enquiry | null = null;
  isLoading = false;
  searchTerm = '';
  filterStatus = '';

  get adminUsername(): string {
    return this.auth.getUsername() || 'Admin';
  }

  get adminInitial(): string {
    return this.adminUsername.charAt(0).toUpperCase();
  }

  get filteredList(): Enquiry[] {
    return this.enquiresList.filter(e => {
      const matchSearch = !this.searchTerm ||
        (e.fullName || '').toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        (e.email || '').toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        (e.mobile || '').includes(this.searchTerm);
      const matchStatus = !this.filterStatus ||
        (e.status || 'Pending') === this.filterStatus;
      return matchSearch && matchStatus;
    });
  }

  get pendingCount():  number { return this.enquiresList.filter(e => !e.status || e.status === 'Pending').length; }
  get resolvedCount(): number { return this.enquiresList.filter(e => e.status === 'Resolved').length; }

  constructor(private api: Api, private auth: AuthService, private router: Router, private toast: ToastService) {}

  ngOnInit() { this.loadsEnquires(); }

  loadsEnquires() {
    this.isLoading = true;
    const sub = this.api.getEnquiresWithNames().subscribe({
      next: (data) => { this.enquiresList = data; this.isLoading = false; },
      error: () => { this.isLoading = false; this.toast.error('Failed to load enquiries.'); }
    });
    this.destroyRef.onDestroy(() => sub.unsubscribe());
  }

  viewEnquiry(enquiry: Enquiry)  { this.selectedEnquiry = enquiry; }
  closeEnquiry()             { this.selectedEnquiry = null; }

  getStatusClass(status: string): string {
    switch (status) {
      case 'In Progress': return 'badge bg-info text-dark';
      case 'Resolved': return 'badge bg-success';
      case 'Closed': return 'badge bg-secondary';
      default: return 'badge bg-warning text-dark';
    }
  }

  onStatusChange(enquiry: Enquiry, event: Event) {
    const select = event.target as HTMLSelectElement;
    const newStatus = select.value;

    const sub = this.api.updateStatus(enquiry.id, newStatus).subscribe({
      next: () => {
        enquiry.status = newStatus;
        this.toast.success(`Status updated to ${newStatus}`);
      },
      error: () => {
        this.toast.error('Failed to update status.');
        select.value = enquiry.status || 'Pending';
      }
    });
    this.destroyRef.onDestroy(() => sub.unsubscribe());
  }

  exportToCsv() {
    const headers = ['ID', 'Full Name', 'Email', 'Mobile', 'Service', 'Subject', 'Status', 'Date'];
    const rows = this.filteredList.map(e => [
      e.id,
      `"${(e.fullName || '').replace(/"/g, '""')}"`,
      `"${(e.email || '').replace(/"/g, '""')}"`,
      e.mobile || '',
      `"${(e.serviceName || '').replace(/"/g, '""')}"`,
      `"${(e.subject || '').replace(/"/g, '""')}"`,
      e.status || 'Pending',
      e.createdate ? new Date(e.createdate).toLocaleDateString() : ''
    ]);
    const csv = [headers.join(','), ...rows.map(r => r.join(','))].join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `enquiries_${new Date().toISOString().slice(0,10)}.csv`;
    a.click();
    URL.revokeObjectURL(url);
    this.toast.success(`Exported ${this.filteredList.length} enquiries to CSV`);
  }

  deleteEnquire(id: number) {
    if (!confirm('Are you sure you want to delete this enquiry?')) return;
    const sub = this.api.deleteEnquiry(id).subscribe({
      next: () => { this.toast.success('Enquiry deleted.'); this.loadsEnquires(); },
      error: () => this.toast.error('Failed to delete enquiry.')
    });
    this.destroyRef.onDestroy(() => sub.unsubscribe());
  }

  logout() { this.auth.logout(); this.router.navigate(['/login']); }
}
