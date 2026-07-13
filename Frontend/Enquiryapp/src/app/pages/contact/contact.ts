import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';

@Component({
  selector: 'app-contact',
  imports: [CommonModule, FormsModule],
  templateUrl: './contact.html',
  styleUrl: './contact.css',
})
export class Contact {
  isSending = false;
  submitted = false;
  hasError = false;

  formData = {
    fullName: '',
    email: '',
    phone: '',
    subject: '',
    message: '',
  };

  get isAdmin(): boolean {
    return !!localStorage.getItem('token') && localStorage.getItem('role') === 'Admin';
  }

  sendMessage(form: NgForm): void {
    if (this.isSending) return;
    this.isSending = true;
    this.submitted = false;
    this.hasError = false;

    // Simulate sending (replace with real API call when backend endpoint is ready)
    setTimeout(() => {
      this.isSending = false;
      this.submitted = true;
      this.formData = { fullName: '', email: '', phone: '', subject: '', message: '' };
      form.resetForm();
    }, 1500);
  }

  resetForm(form: NgForm): void {
    form.resetForm();
    this.formData = { fullName: '', email: '', phone: '', subject: '', message: '' };
    this.submitted = false;
    this.hasError = false;
  }

  scrollToTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
