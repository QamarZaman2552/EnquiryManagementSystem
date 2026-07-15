import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Api } from '../../services/api';
import { ToastService } from '../../services/toast.service';
import { AuthService } from '../../services/auth';
import { ContactFormData } from '../../models/interfaces';

@Component({
  selector: 'app-contact',
  imports: [CommonModule, FormsModule],
  templateUrl: './contact.html',
  styleUrl: './contact.css',
})
export class Contact {
  private destroyRef = inject(DestroyRef);

  isSending = false;
  submitted = false;
  hasError = false;

  formData: ContactFormData = {
    fullName: '',
    email: '',
    phone: '',
    subject: '',
    message: '',
  };

  constructor(
    private api: Api,
    private toast: ToastService,
    public auth: AuthService
  ) {}

  get isAdmin(): boolean {
    return this.auth.isAdmin();
  }

  sendMessage(form: NgForm): void {
    if (this.isSending) return;
    this.isSending = true;
    this.submitted = false;
    this.hasError = false;

    const sub = this.api.sendContactMessage(this.formData).subscribe({
      next: () => {
        this.isSending = false;
        this.submitted = true;
        this.formData = { fullName: '', email: '', phone: '', subject: '', message: '' };
        form.resetForm();
        this.toast.success('Message sent successfully!');
      },
      error: () => {
        this.isSending = false;
        this.hasError = true;
        this.toast.error('Failed to send message. Please try again.');
      }
    });

    this.destroyRef.onDestroy(() => sub.unsubscribe());
  }

  resetForm(form: NgForm): void {
    form.resetForm();
    this.formData = { fullName: '', email: '', phone: '', subject: '', message: '' };
    this.submitted = false;
    this.hasError = false;
  }

  copyToClipboard(text: string, label: string): void {
    navigator.clipboard.writeText(text).then(() => {
      this.toast.success(`${label} copied to clipboard!`);
    });
  }

  scrollToTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
