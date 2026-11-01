import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { LucideAngularModule, Plus, Pencil, Trash2, X, Eye, EyeOff } from 'lucide-angular';
import { PaymentMethodService } from '../../../core/services/payment-method';
import { ToastService } from '../../../core/services/toast';
import { AdminPaymentMethod, PaymentMethodType } from '../../../core/models/payment-method.model';

@Component({
  selector: 'app-admin-payment-methods',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LucideAngularModule],
  templateUrl: './admin-payment-methods.html',
  styleUrl: './admin-payment-methods.css'
})
export class AdminPaymentMethods implements OnInit {
  readonly PlusIcon = Plus;
  readonly EditIcon = Pencil;
  readonly TrashIcon = Trash2;
  readonly CloseIcon = X;
  readonly EyeIcon = Eye;
  readonly EyeOffIcon = EyeOff;

  methods = signal<AdminPaymentMethod[]>([]);
  isLoading = signal<boolean>(true);
  deletingId = signal<number | null>(null);
  togglingId = signal<number | null>(null);

  // ---- النافذة ----
  isModalOpen = signal<boolean>(false);
  editingMethod = signal<AdminPaymentMethod | null>(null);
  isSaving = signal<boolean>(false);

  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private paymentMethodService: PaymentMethodService,
    private toastService: ToastService
  ) {
    this.form = this.fb.group({
      methodName: ['', [Validators.required, Validators.maxLength(100)]],
      type: ['Manual' as PaymentMethodType, Validators.required],
      providerIntegrationId: [''],
      transferUrl: ['', [Validators.maxLength(500), Validators.pattern(/^https?:\/\/\S+$/i)]]
    });
  }

  ngOnInit(): void {
    this.loadMethods();
  }

  private loadMethods(): void {
    this.isLoading.set(true);

    this.paymentMethodService.getAllForAdmin().subscribe({
      next: (response) => {
        this.methods.set(response.data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  // ---- النافذة ----
  openCreate(): void {
    this.editingMethod.set(null);
    this.form.get('type')?.enable();
    this.form.reset({ methodName: '', type: 'Manual', providerIntegrationId: '', transferUrl: '' });
    this.applyTypeRules('Manual');
    this.isModalOpen.set(true);
  }

  openEdit(method: AdminPaymentMethod): void {
    this.editingMethod.set(method);
    this.form.reset({
      methodName: method.methodName,
      type: method.type,
      providerIntegrationId: method.providerIntegrationId ?? '',
      transferUrl: method.transferUrl ?? ''
    });
    this.form.get('type')?.disable(); // نوع الطريقة مبيتغيّرش بعد الإنشاء
    this.applyTypeRules(method.type);
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  setType(type: PaymentMethodType): void {
    this.form.patchValue({ type });
    this.applyTypeRules(type);
  }

  // البوابة: رقم تكامل مطلوب. التحويل اليدوي: رابط تحويل اختياري
  private applyTypeRules(type: PaymentMethodType): void {
    const integration = this.form.get('providerIntegrationId');
    const transfer = this.form.get('transferUrl');
    if (!integration || !transfer) return;

    if (type === 'Gateway') {
      integration.setValidators([Validators.required, Validators.pattern(/^[0-9]+$/), Validators.maxLength(100)]);
      transfer.setValue('');
    } else {
      integration.clearValidators();
      integration.setValue('');
    }

    integration.updateValueAndValidity();
    transfer.updateValueAndValidity();
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue(); // getRawValue عشان النوع لو كان مقفول وقت التعديل
    const editing = this.editingMethod();
    const isGateway = value.type === 'Gateway';

    const integrationId = isGateway ? String(value.providerIntegrationId).trim() : null;
    const transferUrl = isGateway ? null : (String(value.transferUrl ?? '').trim() || null);

    this.isSaving.set(true);

    const request = editing
      ? this.paymentMethodService.update(editing.id, {
          methodName: value.methodName.trim(),
          providerIntegrationId: integrationId,
          transferUrl
        })
      : this.paymentMethodService.create({
          methodName: value.methodName.trim(),
          type: value.type,
          providerIntegrationId: integrationId,
          transferUrl
        });

    request.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.isModalOpen.set(false);
        this.toastService.show(editing ? 'تم تعديل طريقة الدفع' : 'تمت إضافة طريقة الدفع', 'success');
        this.loadMethods();
      },
      error: () => this.isSaving.set(false)
    });
  }

  // ---- الإخفاء والإظهار ----
  toggleVisibility(method: AdminPaymentMethod): void {
    const hide = !method.isHidden;

    const question = hide
      ? `هل تريد إخفاء "${method.methodName}" عن العملاء؟ لن تظهر في صفحة إتمام الطلب، والطلبات السابقة لن تتأثر.`
      : `هل تريد إظهار "${method.methodName}" للعملاء من جديد؟`;

    if (!confirm(question)) return;

    this.togglingId.set(method.id);

    this.paymentMethodService.setVisibility(method.id, hide).subscribe({
      next: () => {
        this.togglingId.set(null);
        this.methods.update(list => list.map(m => (m.id === method.id ? { ...m, isHidden: hide } : m)));
        this.toastService.show(hide ? 'تم إخفاء طريقة الدفع عن العملاء' : 'تم إظهار طريقة الدفع للعملاء', 'success');
      },
      error: () => this.togglingId.set(null)
    });
  }

  delete(method: AdminPaymentMethod): void {
    if (!confirm(`هل تريد حذف طريقة الدفع "${method.methodName}"؟`)) return;

    this.deletingId.set(method.id);

    this.paymentMethodService.delete(method.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.toastService.show('تم حذف طريقة الدفع', 'success');
        this.loadMethods();
      },
      error: () => this.deletingId.set(null)
    });
  }
}