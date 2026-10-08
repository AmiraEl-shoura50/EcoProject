import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { OrderService } from '../../../core/services/order';
import { ToastService } from '../../../core/services/toast';

@Component({
  selector: 'app-payment-proof',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './payment-proof.html',
  styleUrl: './payment-proof.css'
})
export class PaymentProof {
  selectedFile = signal<File | null>(null);
  previewUrl = signal<string | null>(null);
  transferReference = '';
  isSubmitting = signal<boolean>(false);

  private orderId: number;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private orderService: OrderService,
    private toastService: ToastService
  ) {
    this.orderId = +this.route.snapshot.params['id'];
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (file) {
      this.selectedFile.set(file);
      this.previewUrl.set(URL.createObjectURL(file));
    }
  }

  submit(): void {
    const file = this.selectedFile();
    if (!file) {
      this.toastService.show('من فضلك اختر صورة إثبات الدفع', 'error');
      return;
    }

    this.isSubmitting.set(true);

    this.orderService.submitPaymentProof(this.orderId, file, this.transferReference || undefined).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.toastService.show('تم رفع إثبات الدفع بنجاح، في انتظار مراجعة البائع', 'success');
        this.router.navigate(['/orders']);
      },
      error: () => this.isSubmitting.set(false)
    });
  }
}