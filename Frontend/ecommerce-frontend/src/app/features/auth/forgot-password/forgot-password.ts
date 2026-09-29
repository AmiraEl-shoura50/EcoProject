import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css'
})
export class ForgotPassword {
  form: FormGroup;
  isLoading = false;
  submitted = false;
  responseMessage = '';

  constructor(private fb: FormBuilder, private authService: AuthService) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading = true;

    this.authService.forgotPassword(this.form.value).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.submitted = true;
        this.responseMessage = res.message || 'تم إرسال الرابط بنجاح';
      },
      error: () => {
        this.isLoading = false;
        // ✅ نفس الرسالة حتى لو فشل - اتساقًا مع سياسة الأمان في الـ Backend
        this.submitted = true;
        this.responseMessage = 'لو الإيميل ده مسجل عندنا، هيوصلك رابط إعادة تعيين كلمة المرور';
      }
    });
  }
}