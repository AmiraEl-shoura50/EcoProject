import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error';
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  toasts = signal<Toast[]>([]);
  private nextId = 0;

  show(message: string, type: 'success' | 'error' = 'error'): void {
    const id = this.nextId++;
    this.toasts.update(current => [...current, { id, message, type }]);

    // بيتشال تلقائيًا بعد 4 ثواني
    setTimeout(() => this.remove(id), 4000);
  }

  remove(id: number): void {
    this.toasts.update(current => current.filter(t => t.id !== id));
  }
}