import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CartService } from '../../../core/services/cart';
import { OrderService } from '../../../core/services/order';
import { PaymentMethodService } from '../../../core/services/payment-method';
import { PaymentMethod } from '../../../core/models/payment-method.model';
import { ToastService } from '../../../core/services/toast';
import { CheckoutResult } from '../../../core/models/order.model';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './checkout.html',
  styleUrl: './checkout.css'
})
export class Checkout implements OnInit {
  paymentMethods = signal<PaymentMethod[]>([]);
  selectedMethodId = signal<number | null>(null);
  isLoading = signal<boolean>(true);
  isProcessing = signal<boolean>(false);

  constructor(
    public cartService: CartService,
    private orderService: OrderService,
    private paymentMethodService: PaymentMethodService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.paymentMethodService.getAll().subscribe({
      next: (response) => {
        this.paymentMethods.set(response.data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  selectMethod(id: number): void {
    this.selectedMethodId.set(id);
  }

  confirmOrder(): void {
    const methodId = this.selectedMethodId();
    if (!methodId) {
      this.toastService.show('من فضلك اختر طريقة الدفع', 'error');
      return;
    }

    this.isProcessing.set(true);

    this.orderService.checkout({ paymentMethodId: methodId }).subscribe({
      next: (response) => {
        this.isProcessing.set(false);
        this.handleCheckoutResult(response.data);
      },
      error: () => this.isProcessing.set(false)
    });
  }

  private handleCheckoutResult(results: CheckoutResult[]): void {
  const gatewayResult = results.find(r => r.paymentUrl);

  if (gatewayResult?.paymentUrl) {
    window.location.href = gatewayResult.paymentUrl;
  } else {
    // ✅ بدل التوجيه لـ /orders، نوديه لصفحة رفع الإثبات مع رقم الأوردر
    const firstOrder = results[0]?.order;
    if (firstOrder) {
      this.router.navigate(['/orders', firstOrder.id, 'payment-proof']);
    } else {
      this.router.navigate(['/orders']);
    }
  }
}
}