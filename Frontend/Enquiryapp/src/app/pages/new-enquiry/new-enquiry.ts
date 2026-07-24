import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../../services/api';
import { AuthService } from '../../services/auth';
import { ToastService } from '../../services/toast.service';
import { Service } from '../../models/interfaces';

@Component({
  selector: 'app-new-enquiry',
  imports: [CommonModule, FormsModule],
  templateUrl: './new-enquiry.html',
  styleUrl: './new-enquiry.css',
})
export class NewEnquiry implements OnInit {
  private destroyRef = inject(DestroyRef);
  private cdr = inject(ChangeDetectorRef);

  currentYear = new Date().getFullYear();

  servicesList: Service[] = [];
  isLoading = false;
  loadError = false;
  isSubmitting = false;
  submitted = false;

  formData = {
    fullName: '',
    email: '',
    mobile: '',
    subject: '',
    message: '',
    serviceId: null as number | null,
  };

  constructor(private api: Api, public auth: AuthService, private toast: ToastService) {}

  ngOnInit() { this.loadServices(); }

  get isAdmin(): boolean {
    return this.auth.isAdmin();
  }

  loadServices() {
    this.isLoading = true;
    this.loadError = false;
    const sub = this.api.getServices().subscribe({
      next: (data) => {
        this.servicesList = data;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.loadError = true;
        this.cdr.detectChanges();
      }
    });
    this.destroyRef.onDestroy(() => sub.unsubscribe());
  }

  submitEnquiry() {
    if (this.isAdmin) return;
    this.isSubmitting = true;
    const sub = this.api.addNewEnquiry(this.formData).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.submitted = true;
        this.formData = { fullName: '', email: '', mobile: '', subject: '', message: '', serviceId: null };
        this.scrollToTop();
        this.cdr.detectChanges();
      },
      error: () => {
        this.isSubmitting = false;
        this.toast.error('Failed to submit enquiry. Please try again.');
        this.cdr.detectChanges();
      }
    });
    this.destroyRef.onDestroy(() => sub.unsubscribe());
  }

  scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
