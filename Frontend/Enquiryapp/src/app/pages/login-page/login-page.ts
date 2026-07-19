import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-login-page',
  imports: [CommonModule, FormsModule],
  templateUrl: './login-page.html',
  styleUrl: './login-page.css',
})
export class LoginPage {
  username = '';
  password = '';
  loginError = false;
  isLoading = false;
  currentYear = new Date().getFullYear();

  constructor(private router: Router, private auth: AuthService) { }

  onSubmit() {
    this.loginError = false;
    this.isLoading = true;
    this.auth.login(this.username, this.password).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/admin/dashboard']);
      },
      error: () => {
        this.isLoading = false;
        this.loginError = true;
      }
    });
  }
}
