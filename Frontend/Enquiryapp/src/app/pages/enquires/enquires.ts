import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Api } from '../../services/api';
import { AuthService } from '../../services/auth';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-enquires',
  imports: [CommonModule],
  templateUrl: './enquires.html',
  styleUrl: './enquires.css',
})
export class Enquires implements OnInit {
  enquiresList: any[] = [];
  selectedEnquiry: any = null;
  isLoading = false;

  get pendingCount():  number { return this.enquiresList.filter(e => !e.status || e.status === 'Pending').length; }
  get resolvedCount(): number { return this.enquiresList.filter(e => e.status === 'Resolved').length; }

  constructor(private api: Api, private auth: AuthService, private router: Router, private toast: ToastService) {}

  ngOnInit() { this.loadsEnquires(); }

  loadsEnquires() {
    this.isLoading = true;
    this.api.getEnquiresWithNames().subscribe({
      next: (data) => { this.enquiresList = data; this.isLoading = false; },
      error: () => { this.isLoading = false; this.toast.error('Failed to load enquiries.'); }
    });
  }

  viewEnquiry(enquiry: any)  { this.selectedEnquiry = enquiry; }
  closeEnquiry()             { this.selectedEnquiry = null; }

  deleteEnquire(id: number) {
    if (!confirm('Are you sure you want to delete this enquiry?')) return;
    this.api.deleteEnquiry(id).subscribe({
      next: () => { this.toast.success('Enquiry deleted.'); this.loadsEnquires(); },
      error: () => this.toast.error('Failed to delete enquiry.')
    });
  }

  logout() { this.auth.logout(); this.router.navigate(['/login']); }
}
