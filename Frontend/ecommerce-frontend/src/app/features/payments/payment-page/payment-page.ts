import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, ExternalLink } from 'lucide-angular';
import { PaymentGroupService } from '../../../core/services/payment-group';
import { PwaInstallService } from '../../../core/services/pwa-install';
import { ToastService } from '../../../core/services/toast';
import { PaymentGroup } from '../../../core/models/payment.model';

@Component({
  selector: 'app-payment-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, LucideAngularModule],
  templateUrl: './payment-page.html',
  styleUrl: './payment-page.css'
})
export class PaymentPage implements OnInit {
  readonly LinkIcon = ExternalLink;

  group = signal<PaymentGroup | null>(null);
  isLoading = signal<boolean>(true);
  selectedFile = signal<File | null>(null);
  previewUrl = signal<string | null>(null);
  isSubmitting = signal<boolean>(false);
  transferReference = '';

  private groupId: number;
  private readonly maxImageSize = 5 * 1024 * 1024; // 5MB

  constructor(
    private route: ActivatedRoute,
    private paymentGroupService: PaymentGroupService,
    public pwa: PwaInstallService,
    private toastService: ToastService
  ) {
    this.groupId = +this.route.snapshot.params['id'];
  }

  ngOnInit(): void {
    this.load();
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

    this.paymentGroupService.submitProof(this.groupId, file, this.transferReference.trim() || undefined).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.toastService.show('تم رفع إثبات الدفع، في انتظار مراجعة الإدارة', 'success');
        this.selectedFile.set(null);
        this.previewUrl.set(null);
        this.transferReference = '';
        this.load();
      },
      error: () => this.isSubmitting.set(false)
    });
  }

  private load(): void {
    this.paymentGroupService.getById(this.groupId).subscribe({
      next: (response) => {
        this.group.set(response.data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}