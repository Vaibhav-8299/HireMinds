import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (toastService.toastState()) {
      <div class="toast-container fadeIn">
        <div class="toast" [ngClass]="getToastClass(toastService.toastState()!.type)">
          {{ toastService.toastState()!.message }}
        </div>
      </div>
    }
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: 90px;
      right: 20px;
      z-index: 9999;
    }
    .toast {
      padding: 14px 24px;
      border-radius: 8px;
      font-weight: 500;
      font-size: 15px;
      box-shadow: 0 10px 25px rgba(0,0,0,0.3);
      color: white;
      display: flex;
      align-items: center;
      gap: 10px;
    }
    .toast-success { background: var(--success); border-left: 4px solid #16a34a; }
    .toast-error { background: var(--danger); border-left: 4px solid #dc2626; }
    .toast-info { background: var(--accent-blue); border-left: 4px solid #2563eb; }
    
    .fadeIn {
      animation: slideInRight 0.3s cubic-bezier(0.2, 0.8, 0.2, 1);
    }
    @keyframes slideInRight {
      from { opacity: 0; transform: translateX(50px); }
      to { opacity: 1; transform: translateX(0); }
    }
  `]
})
export class ToastComponent {
  toastService = inject(ToastService);

  getToastClass(type: string): string {
    return `toast-${type}`;
  }
}
