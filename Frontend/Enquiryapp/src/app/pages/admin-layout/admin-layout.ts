import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-admin-layout',
  imports: [CommonModule],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css',
})
export class AdminLayout {
  @Input() activePage: 'dashboard' | 'enquiries' | 'services' = 'dashboard';

  currentYear = new Date().getFullYear();

  constructor(private auth: AuthService, private router: Router) {}

  get adminUsername(): string { return this.auth.getUsername() || 'Admin'; }
  get adminInitial(): string { return this.adminUsername.charAt(0).toUpperCase(); }

  logout(): void {
    this.auth.logout().subscribe({
      next: () => this.router.navigate(['/login']),
      error: () => this.router.navigate(['/login'])
    });
  }
}
