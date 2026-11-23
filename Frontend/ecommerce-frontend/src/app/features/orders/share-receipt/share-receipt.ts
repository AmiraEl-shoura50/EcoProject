import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PaymentGroupService } from '../../../core/services/payment-group';
import { ToastService } from '../../../core/services/toast';
import { PendingPayment } from '../../../core/models/payment.model';

const SHARE_CACHE = 'shared-receipts-v1';

@Component({
  selector: 'app-share-receipt',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './share-receipt.html',
  styleUrl: './share-receipt.css'
})
export class ShareReceipt implements OnInit {
  isLoading = signal<boolean>(true);
  file = signal<File | null>(null);
  previewUrl = signal<string | null>(null);
  payments = signal<PendingPayment[]>([]);
  selectedPaymentId = signal<number | null>(null);
  isSubmitting = signal<boolean>(false);
  transferReference = '';

  private readonly maxImageSize = 5 * 1024 * 1024; // 5MB

  constructor(
    private paymentGroupService: PaymentGroupService,
    private toastService: ToastService,
    private router: Router
  ) {}

  async ngOnInit(): Promise<void> {
    await this.readShared();

    // الدفعات اللي لسه في انتظار الدفع
    this.paymentGroupService.getPending().subscribe({
      next: (response) => {
        this.payments.set(response.data);
        if (response.data.length === 1) this.selectedPaymentId.set(response.data[0].id);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  selectPayment(paymentId: number): void {
    this.selectedPaymentId.set(paymentId);
  }

  async discard(): Promise<void> {
    await this.clearShared();
    this.router.navigate(['/orders']);
  }

  submit(): void {
    const file = this.file();
    const paymentId = this.selectedPaymentId();

    if (!file) return;
    if (!paymentId) {
      this.toastService.show('اختر الدفعة التي تريد إرفاق الإيصال بها', 'error');
      return;
    }
    if (!file.type.startsWith('image/')) {
      this.toastService.show('الملف المشارك ليس صورة', 'error');
      return;
    }
    if (file.size > this.maxImageSize) {
      this.toastService.show('حجم الصورة كبير، الحد الأقصى 5 ميجابايت', 'error');
      return;
    }

    this.isSubmitting.set(true);

    this.paymentGroupService.submitProof(paymentId, file, this.transferReference.trim() || undefined).subscribe({
      next: async () => {
        this.isSubmitting.set(false);
        await this.clearShared();
        this.toastService.show('تم رفع إثبات الدفع، في انتظار مراجعة الإدارة', 'success');
        this.router.navigate(['/payments', paymentId]);
      },
      error: () => this.isSubmitting.set(false)
    });
  }

  // بنقرا الإيصال اللي الـ Service Worker خزّنه لما التطبيق شاركه
  private async readShared(): Promise<void> {
    try {
      const cache = await caches.open(SHARE_CACHE);

      const fileResponse = await cache.match('/__shared-file');
      if (fileResponse) {
        const blob = await fileResponse.blob();
        const name = decodeURIComponent(fileResponse.headers.get('X-File-Name') ?? 'receipt');

        this.file.set(new File([blob], name, { type: blob.type }));
        this.previewUrl.set(URL.createObjectURL(blob));
      }

      // لو التطبيق شارك نص (زي رقم العملية)، نحطه في خانة المرجع
      const textResponse = await cache.match('/__shared-text');
      if (textResponse) {
        const text = (await textResponse.text()).trim();
        if (text && text.length <= 100) this.transferReference = text;
      }
    } catch {
      // مفيش تخزين متاح: الصفحة هتعرض حالة "مفيش إيصال"
    }
  }

  private async clearShared(): Promise<void> {
    try {
      const cache = await caches.open(SHARE_CACHE);
      await cache.delete('/__shared-file');
      await cache.delete('/__shared-text');
    } catch {
      // مش حرج
    }
  }
}