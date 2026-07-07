import { CommonModule } from '@angular/common';
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { Api } from '../../services/api';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-enquires',
  imports: [CommonModule],
  templateUrl: './enquires.html',
  styleUrl: './enquires.css',
})
export class Enquires implements OnInit {
  enquiresList: any[] = [];
  selectedEnquiry: any = null;

  get pendingCount(): number {
    return this.enquiresList.filter(e => !e.status || e.status === 'Pending').length;
  }

  get resolvedCount(): number {
    return this.enquiresList.filter(e => e.status === 'Resolved').length;
  }

  constructor(
    private api: Api,
    private cdr: ChangeDetectorRef,
    private auth: AuthService,
    private router: Router
  ) { }

  ngOnInit() {
    this.loadsEnquires();
  }

  loadsEnquires() {
    console.log('loadsEnquires: fetching from', this.api.getEnquiresWithNames);
    this.api.getEnquiresWithNames().subscribe({
      next: (data) => {
        console.log('loadsEnquires: success', data);
        this.enquiresList = data;
        this.cdr.detectChanges(); // Force template to render the new data
      },
      error: (err) => {
        console.error('loadsEnquires: error', err);
      }
    });
  }

  viewEnquiry(enquiry: any) {
    this.selectedEnquiry = enquiry;
  }

  closeEnquiry() {
    this.selectedEnquiry = null;
  }

  deleteEnquire(id: number) {
    if (!confirm('Are you sure you want to delete this enquiry?')) return;
    this.api.deleteEnquiry(id).subscribe({
      next: () => {
        alert('Enquiry deleted successfully');
        this.loadsEnquires();
      },
      error: () => {
        alert('Error deleting enquiry');
      }
    });
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
