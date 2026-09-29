import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { LucideAngularModule, Eye, EyeOff } from 'lucide-angular';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, LucideAngularModule],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  registerForm: FormGroup;
  errorMessage = '';
  isLoading = false;
  showPassword = false;

  readonly EyeIcon = Eye;
  readonly EyeOffIcon = EyeOff;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      phoneNumber: ['', Validators.required],
      role: ['Customer', Validators.required],
      address: [''],
      storeName: ['']
    });

    // كل مرة الدور يتغيّر، نحدّث الـ Validators المطلوبة
    this.registerForm.get('role')?.valueChanges.subscribe(role => {
      this.updateConditionalValidators(role);
    });

    // نطبّق الشرط من البداية (Customer هو الافتراضي)
    this.updateConditionalValidators('Customer');
  }

  private updateConditionalValidators(role: string): void {
    const addressControl = this.registerForm.get('address');
    const storeNameControl = this.registerForm.get('storeName');

    if (role === 'Customer') {
      addressControl?.setValidators([Validators.required]);
      storeNameControl?.clearValidators();
    } else {
      storeNameControl?.setValidators([Validators.required]);
      addressControl?.clearValidators();
    }

    addressControl?.updateValueAndValidity();
    storeNameControl?.updateValueAndValidity();
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.register(this.registerForm.value).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigateByUrl('/');
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'حصل خطأ أثناء إنشاء الحساب';
      }
    });
  }
}