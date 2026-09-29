import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { LucideAngularModule, Eye, EyeOff } from 'lucide-angular';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, LucideAngularModule],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.css'
})
export class ResetPassword implements OnInit {
  form: FormGroup;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  showPassword = false;

  private email = '';
  private token = '';

  readonly EyeIcon = Eye;
  readonly EyeOffIcon = EyeOff;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit(): void {
    // ✅ نقرأ الـ email والـ token من الرابط اللي جاي من الإيميل
    this.email = this.route.snapshot.queryParams['email'] || '';
    this.token = this.route.snapshot.queryParams['token'] || '';

    if (!this.email || !this.token) {
      this.errorMessage = 'الرابط غير صالح، تأكد إنك فتحت الرابط اللي وصلك بالإيميل كامل';
    }
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit(): void {
    if (this.form.invalid || !this.email || !this.token) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.resetPassword({
      email: this.email,
      token: this.token,
      newPassword: this.form.value.newPassword
    }).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.successMessage = res.message || 'تم تغيير كلمة المرور بنجاح';
        setTimeout(() => this.router.navigate(['/login']), 2500);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'حصل خطأ، جرب تطلب رابط جديد';
      }
    });
  }
}