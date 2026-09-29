import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { LucideAngularModule, Eye, EyeOff } from 'lucide-angular';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, LucideAngularModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  loginForm: FormGroup;
  errorMessage = '';
  isLoading = false;
  showPassword = false;
  private returnUrl = '/';

  readonly EyeIcon = Eye;
  readonly EyeOffIcon = EyeOff;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

 onSubmit(): void {
  if (this.loginForm.invalid) {
    this.loginForm.markAllAsTouched();
    return;
  }

  this.isLoading = true;

  this.authService.login(this.loginForm.value).subscribe({
    next: () => {
      this.isLoading = false;
      this.router.navigateByUrl(this.returnUrl);
    },
    error: () => {
      this.isLoading = false;
      // الـ Toast اتعرضت أوتوماتيك من الـ Interceptor، هنا بس بنوقف الـ Loading
    }
  });
}
}