import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from './toast.service';

@Component({
  selector: 'app-toast-container',
  imports: [CommonModule],
  template: `
    <div class="toast-container">
      <div *ngFor="let t of toastService.toasts()" class="toast-custom toast-{{ t.type }}" role="alert">
        <i class="bi" [class.bi-check-circle-fill]="t.type === 'success'"
           [class.bi-exclamation-circle-fill]="t.type === 'error'"
           [class.bi-exclamation-triangle-fill]="t.type === 'warning'"
           [class.bi-info-circle-fill]="t.type === 'info'"></i>
        <span>{{ t.message }}</span>
        <button class="toast-close" (click)="toastService.remove(t.id)" aria-label="Close">&times;</button>
      </div>
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 8px;
      max-width: 400px;
    }
    .toast-custom {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 12px 16px;
      border-radius: 10px;
      background: #fff;
      box-shadow: 0 8px 24px rgba(0,0,0,0.12);
      font-size: 0.9rem;
      animation: slideIn 0.3s ease;
      border-left: 4px solid;
    }
    .toast-custom i { font-size: 1.2rem; flex-shrink: 0; }
    .toast-custom span { flex: 1; color: #1e293b; }
    .toast-close {
      background: none;
      border: none;
      font-size: 1.3rem;
      cursor: pointer;
      color: #94a3b8;
      padding: 0;
      line-height: 1;
    }
    .toast-close:hover { color: #475569; }
    .toast-success { border-left-color: #22c55e; }
    .toast-success i { color: #22c55e; }
    .toast-error { border-left-color: #ef4444; }
    .toast-error i { color: #ef4444; }
    .toast-warning { border-left-color: #f59e0b; }
    .toast-warning i { color: #f59e0b; }
    .toast-info { border-left-color: #3b82f6; }
    .toast-info i { color: #3b82f6; }
    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }
  `]
})
export class ToastContainer {
  constructor(public toastService: ToastService) {}
}
