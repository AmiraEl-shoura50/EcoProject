import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, ExternalLink } from 'lucide-angular';
import { OrderService } from '../../../core/services/order';
import { PwaInstallService } from '../../../core/services/pwa-install';
import { ToastService } from '../../../core/services/toast';
import { Order } from '../../../core/models/order.model';

@Component({
  selector: 'app-payment-proof',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, LucideAngularModule],
  templateUrl: './payment-proof.html',
  styleUrl: './payment-proof.css'
})
export class PaymentProof implements OnInit {
  readonly LinkIcon = ExternalLink;

  order = signal<Order | null>(null);
  isLoading = signal<boolean>(true);
  selectedFile = signal<File | null>(null);
  previewUrl = signal<string | null>(null);
  isSubmitting = signal<boolean>(false);
  transferReference = '';

  // الطلب لسه محتاج إثبات: في انتظار الدفع، وبطريقة تحويل يدوي
  canUpload = computed(() => {
    const order = this.order();
    return !!order && order.status === 'Pending' && order.paymentMethodType === 'Manual';
  });

  private orderId: number;
  private readonly maxImageSize = 5 * 1024 * 1024; // 5MB

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private orderService: OrderService,
    public pwa: PwaInstallService,
    private toastService: ToastService
  ) {
    this.orderId = +this.route.snapshot.params['id'];
  }

  ngOnInit(): void {
    this.orderService.getById(this.orderId).subscribe({
      next: (response) => {
        this.order.set(response.data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      this.toastService.show('من فضلك اختر ملف صورة فقط', 'error');
      input.value = '';
      return;
    }

    if (file.size > this.maxImageSize) {
      this.toastService.show('حجم الصورة كبير، الحد الأقصى 5 ميجابايت', 'error');
      input.value = '';
      return;
    }

    this.selectedFile.set(file);
    this.previewUrl.set(URL.createObjectURL(file));
  }

  install(): void {
    this.pwa.install();
  }

  submit(): void {
    const file = this.selectedFile();
    if (!file) {
      this.toastService.show('من فضلك اختر صورة إثبات الدفع', 'error');
      return;
    }

    this.isSubmitting.set(true);

    this.orderService.submitPaymentProof(this.orderId, file, this.transferReference.trim() || undefined).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.toastService.show('تم رفع إثبات الدفع بنجاح، في انتظار مراجعة البائع', 'success');
        this.router.navigate(['/orders']);
      },
      error: () => this.isSubmitting.set(false)
    });
  }
}