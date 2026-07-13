import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../../services/api';
import { AuthService } from '../../services/auth';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-new-enquiry',
  imports: [CommonModule, FormsModule],
  templateUrl: './new-enquiry.html',
  styleUrl: './new-enquiry.css',
})
export class NewEnquiry implements OnInit {
  servicesList: any[] = [];
  isLoading = false;
  isSubmitting = false;

  formData = {
    fullName: '',
    email: '',
    mobile: '',
    subject: '',
    message: '',
    serviceId: 0,
  };

  constructor(private api: Api, public auth: AuthService, private toast: ToastService) {}

  ngOnInit() { this.loadServices(); }

  get isAdmin(): boolean {
    return !!localStorage.getItem('token') && localStorage.getItem('role') === 'Admin';
  }

  loadServices() {
    this.isLoading = true;
    this.api.getServices().subscribe({
      next: (data) => { this.servicesList = data; this.isLoading = false; },
      error: () => { this.isLoading = false; }
    });
  }

  submitEnquiry() {
    if (this.isAdmin) return;
    this.isSubmitting = true;
    this.api.addNewEnquiry(this.formData).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.toast.success('Enquiry submitted successfully! We will contact you shortly.');
        this.formData = { fullName: '', email: '', mobile: '', subject: '', message: '', serviceId: 0 };
      },
      error: () => {
        this.isSubmitting = false;
        this.toast.error('Failed to submit enquiry. Please try again.');
      }
    });
  }

  scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
