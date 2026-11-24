import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, ImagePlus, Trash2, ExternalLink } from 'lucide-angular';
import { SellerService } from '../../../core/services/seller';
import { ToastService } from '../../../core/services/toast';
import { SellerProfile } from '../../../core/models/seller.model';
import { Stars } from '../../../shared/components/stars/stars';

@Component({
  selector: 'app-seller-store',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, LucideAngularModule, Stars],
  templateUrl: './seller-store.html',
  styleUrl: './seller-store.css'
})
export class SellerStore implements OnInit {
  readonly ImageIcon = ImagePlus;
  readonly TrashIcon = Trash2;
  readonly LinkIcon = ExternalLink;

  // جوه الكلاس: خيارات طريقة التحويل
readonly payoutMethods = [
  { value: '', label: 'غير محدد' },
  { value: 'VodafoneCash', label: 'فودافون كاش' },
  { value: 'InstaPay', label: 'إنستاباي' },
  { value: 'BankAccount', label: 'حساب بنكي' }
];

  profile = signal<SellerProfile | null>(null);
  isLoading = signal<boolean>(true);
  isSaving = signal<boolean>(false);

  selectedFile = signal<File | null>(null);
  previewUrl = signal<string | null>(null);
  removeImage = signal<boolean>(false);

  // الصورة المعروضة: المختارة حديثًا، وإلا الحالية (إلا لو البائع قرر يمسحها)
  shownImage = computed(() => {
    const preview = this.previewUrl();
    if (preview) return preview;
    return this.removeImage() ? null : (this.profile()?.imageUrl ?? null);
  });

  form: FormGroup;

  private readonly maxImageSize = 5 * 1024 * 1024; // 5MB

  constructor(
    private fb: FormBuilder,
    private sellerService: SellerService,
    private toastService: ToastService
  ) {
       this.form = this.fb.group({
      storeName: ['', [Validators.required, Validators.maxLength(150)]],
      payoutMethod: [''],
      payoutAccount: ['', [Validators.maxLength(100)]]
    });
  }

  ngOnInit(): void {
    this.load();
  }

  hasChanges(): boolean {
    return this.form.dirty || !!this.selectedFile() || this.removeImage();
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
    this.removeImage.set(false);
  }

  removeLogo(): void {
    if (this.selectedFile()) {
      // لسه مرفعتش الصورة المختارة: نلغيها ونرجع للحالية
      this.selectedFile.set(null);
      this.previewUrl.set(null);
    } else {
      this.removeImage.set(true);
    }
  }

  undoRemove(): void {
    this.removeImage.set(false);
  }

  // نفس قواعد الباك: الطريقة والحساب مع بعض، وفودافون كاش رقم موبايل مصري
  payoutError(): string | null {
    const method = String(this.form.value.payoutMethod ?? '');
    const account = String(this.form.value.payoutAccount ?? '').trim();

    if (!method && !account) return null;
    if (!method) return 'اختر طريقة التحويل';
    if (!account) return 'أدخل رقم أو عنوان التحويل';
    if (method === 'VodafoneCash' && !/^01[0125][0-9]{8}$/.test(account)) {
      return 'رقم فودافون كاش غير صحيح (11 رقمًا يبدأ بـ 01)';
    }

    return null;
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payoutError = this.payoutError();
    if (payoutError) {
      this.toastService.show(payoutError, 'error');
      return;
    }

    // الأسماء بحرف كبير عشان تطابق UpdateSellerDto
    const formData = new FormData();
    formData.append('StoreName', this.form.value.storeName.trim());
    formData.append('PayoutMethod', String(this.form.value.payoutMethod ?? ''));
    formData.append('PayoutAccount', String(this.form.value.payoutAccount ?? '').trim());

    const file = this.selectedFile();
    if (file) {
      formData.append('Image', file);
    } else if (this.removeImage()) {
      formData.append('RemoveImage', 'true');
    }

    this.isSaving.set(true);

    this.sellerService.update(formData).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.toastService.show('تم حفظ بيانات المتجر بنجاح', 'success');
        this.load();
      },
      error: () => this.isSaving.set(false) // رسالة الخطأ (زي اسم مكرر) بتظهر من الـ Interceptor
    });
  
  }

  private load(): void {
    this.sellerService.getMine().subscribe({
      next: (response) => {
        this.profile.set(response.data);
        this.form.reset({
          storeName: response.data.storeName,
          payoutMethod: response.data.payoutMethod ?? '',
          payoutAccount: response.data.payoutAccount ?? ''
        });
        this.selectedFile.set(null);
        this.previewUrl.set(null);
        this.removeImage.set(false);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}