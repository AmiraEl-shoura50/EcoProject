import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, Package, Heart, ShoppingCart } from 'lucide-angular';
import { CustomerService } from '../../../core/services/customer';
import { ToastService } from '../../../core/services/toast';
import { Customer } from '../../../core/models/customer.model';

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

  customer = signal<Customer | null>(null);
  isLoading = signal<boolean>(true);
  isSaving = signal<boolean>(false);

  initial = computed(() => this.customer()?.firstName?.charAt(0) ?? '');

  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private customerService: CustomerService,
    private toastService: ToastService
  ) {
    this.form = this.fb.group({
      address: ['', [Validators.required, Validators.maxLength(300)]]
    });
  }

  ngOnInit(): void {
    this.customerService.getMe().subscribe({
      next: (response) => {
        this.customer.set(response.data);
        this.form.patchValue({ address: response.data.address });
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);

    this.customerService.updateMe(this.form.value.address).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.form.markAsPristine();
        this.toastService.show('تم حفظ التعديلات بنجاح', 'success');
      },
      error: () => this.isSaving.set(false)
    });
  }
}