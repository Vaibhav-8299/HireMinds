import { Injectable, signal } from '@angular/core';

export interface ToastMessage {
  message: string;
  type: 'success' | 'error' | 'info';
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  toastState = signal<ToastMessage | null>(null);
  private timeoutId: any;

  show(message: string, type: 'success' | 'error' | 'info' = 'info') {
    this.toastState.set({ message, type });

    if (this.timeoutId) {
      clearTimeout(this.timeoutId);
    }

    // Auto dismiss after 3 seconds
    this.timeoutId = setTimeout(() => {
      this.toastState.set(null);
    }, 3000);
  }

  showSuccess(message: string) {
    this.show(message, 'success');
  }

  showError(message: string) {
    this.show(message, 'error');
  }

  clear() {
    this.toastState.set(null);
  }
}
