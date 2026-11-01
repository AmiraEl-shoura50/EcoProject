import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, Package, Heart, ShoppingCart, Eye, EyeOff } from 'lucide-angular';

import { AuthService } from '../../../core/services/auth';
import { ToastService } from '../../../core/services/toast';
import { Customer } from '../../../core/models/customer.model';
import { CustomerService } from '../../../core/services/customer';

// ✅ بتتأكد إن تأكيد كلمة المرور مطابق للجديدة
function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const newPassword = group.get('newPassword')?.value;
  const confirmPassword = group.get('confirmPassword')?.value;
  return newPassword === confirmPassword ? null : { mismatch: true };
}

@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, LucideAngularModule],
  templateUrl: './profile-page.html',
  styleUrl: './profile-page.css'
})
export class ProfilePage implements OnInit {
  readonly OrdersIcon = Package;
  readonly HeartIcon = Heart;
  readonly CartIcon = ShoppingCart;
  readonly EyeIcon = Eye;
  readonly EyeOffIcon = EyeOff;

  customer = signal<Customer | null>(null);
  isLoading = signal<boolean>(true);
  isSavingProfile = signal<boolean>(false);
  isSavingPassword = signal<boolean>(false);
  showPasswords = signal<boolean>(false);

  initial = computed(() => this.customer()?.firstName?.charAt(0) ?? '');

  profileForm: FormGroup;
  passwordForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private customerService: CustomerService,
    private authService: AuthService,
    private toastService: ToastService
  ) {
    this.profileForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.maxLength(50)]],
      phoneNumber: ['', [Validators.required, Validators.pattern(/^\+?[0-9]{8,15}$/)]],
      address: ['', [Validators.required, Validators.maxLength(300)]]
    });

    this.passwordForm = this.fb.group(
      {
        currentPassword: ['', Validators.required],
        newPassword: ['', [Validators.required, Validators.minLength(6)]],
        confirmPassword: ['', Validators.required]
      },
      { validators: passwordsMatch }
    );
  }

  ngOnInit(): void {
    this.customerService.getMe().subscribe({
      next: (response) => {
        const c = response.data;
        this.customer.set(c);
        this.profileForm.patchValue({
          firstName: c.firstName,
          lastName: c.lastName,
          phoneNumber: c.phoneNumber ?? '',
          address: c.address
        });
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  togglePasswords(): void {
    this.showPasswords.update(v => !v);
  }

  saveProfile(): void {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.isSavingProfile.set(true);
    const value = this.profileForm.getRawValue();

    this.customerService.updateMe(value).subscribe({
      next: () => {
        this.isSavingProfile.set(false);
        this.customer.update(c => (c ? { ...c, ...value } : c)); // ✅ بطاقة المستخدم تتحدّث فورًا
        this.profileForm.markAsPristine();
        this.toastService.show('تم حفظ التعديلات بنجاح', 'success');
      },
      error: () => this.isSavingProfile.set(false)
    });
  }

  changePassword(): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    this.isSavingPassword.set(true);
    const { currentPassword, newPassword } = this.passwordForm.value;

    this.authService.changePassword({ currentPassword, newPassword }).subscribe({
      next: () => {
        this.isSavingPassword.set(false);
        this.passwordForm.reset();
        this.toastService.show('تم تغيير كلمة المرور بنجاح', 'success');
      },
      error: () => this.isSavingPassword.set(false)
    });
  }
}