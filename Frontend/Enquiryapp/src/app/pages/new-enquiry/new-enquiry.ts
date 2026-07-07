import { CommonModule } from '@angular/common';
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../../services/api';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-new-enquiry',
  imports: [CommonModule, FormsModule],
  templateUrl: './new-enquiry.html',
  styleUrl: './new-enquiry.css',
})
export class NewEnquiry implements OnInit {
  servicesList: any[] = [];

  // form-Fields
  formData = {
    fullName: '',
    email: '',
    mobile: '',
    subject: '',
    message: '',
    serviceId: 0,
  };

  constructor(private api: Api, private cdr: ChangeDetectorRef, public auth: AuthService) { }

  ngOnInit() {
    this.loadServices();
  }

  get isAdmin(): boolean {
    return this.auth.isLoggedIn();
  }

  loadServices() {
    this.api.getServices().subscribe({
      next: (data) => {
        this.servicesList = data;
        this.cdr.detectChanges(); // Force template dropdown to populate
      },
      error: (err) => {
        console.error('Error loading services', err);
      }
    });
  }

  submitEnquiry() {
    if (this.isAdmin) {
      alert('Administrators cannot submit enquiries.');
      return;
    }
    this.api.addNewEnquiry(this.formData).subscribe({
      next: (res) => {
        alert('Enquiry submitted successfully');
        this.formData = { fullName: '', email: '', mobile: '', subject: '', message: '', serviceId: 0 }; //reset form
      },
      error: (err) => {
        alert('Error submitting enquiry');
      }
    });
  }
}
