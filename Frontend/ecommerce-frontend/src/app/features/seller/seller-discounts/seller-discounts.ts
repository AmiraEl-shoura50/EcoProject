import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators
} from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, catchError, debounceTime, distinctUntilChanged, map, of, switchMap } from 'rxjs';
import { LucideAngularModule, Plus, Trash2, X, Search } from 'lucide-angular';
import { DiscountService } from '../../../core/services/discount';
import { ProductService } from '../../../core/services/product';
import { CategoryService } from '../../../core/services/category';
import { ToastService } from '../../../core/services/toast';
import {
  CreateDiscountRequest, Discount, DiscountTargetType, DiscountType
} from '../../../core/models/discount.model';
import { Product } from '../../../core/models/product.model';

type StatusKey = 'active' | 'scheduled' | 'expired';
type FilterKey = 'all' | StatusKey;

// ✅ تحقق على مستوى الفورم كله: ترتيب التواريخ، وانتهاء في المستقبل، وحد النسبة
function discountValidator(group: AbstractControl): ValidationErrors | null {
  const errors: ValidationErrors = {};

  const start = new Date(group.get('startDate')?.value).getTime();
  const end = new Date(group.get('endDate')?.value).getTime();

  if (!isNaN(start) && !isNaN(end) && end <= start) errors['dateOrder'] = true;
  if (!isNaN(end) && end <= Date.now()) errors['endPast'] = true;

  const type = group.get('discountType')?.value;
  const amount = Number(group.get('discountAmount')?.value);
  if (type === 'Percentage' && amount >= 100) errors['percentMax'] = true;

  return Object.keys(errors).length > 0 ? errors : null;
}

@Component({
  selector: 'app-seller-discounts',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LucideAngularModule],
  templateUrl: './seller-discounts.html',
  styleUrl: './seller-discounts.css'
})
export class SellerDiscounts implements OnInit {
  readonly PlusIcon = Plus;
  readonly TrashIcon = Trash2;
  readonly CloseIcon = X;
  readonly SearchIcon = Search;

  discounts = signal<Discount[]>([]);
  isLoading = signal<boolean>(true);
  filter = signal<FilterKey>('all');
  deletingId = signal<number | null>(null);

  filters: { key: FilterKey; label: string }[] = [
    { key: 'all', label: 'الكل' },
    { key: 'active', label: 'ساري' },
    { key: 'scheduled', label: 'مجدول' },
    { key: 'expired', label: 'منتهي' }
  ];

  filteredDiscounts = computed(() =>
    this.discounts().filter(d => this.filter() === 'all' || this.statusOf(d) === this.filter())
  );

  // ---- النافذة ----
  isModalOpen = signal<boolean>(false);
  isSaving = signal<boolean>(false);
  form: FormGroup;

  // ---- اختيار المنتج (بحث مباشر في منتجات البائع) ----
  productQuery = signal<string>('');
  productResults = signal<Product[]>([]);
  selectedProduct = signal<Product | null>(null);
  isSearchingProducts = signal<boolean>(false);
  private productSearch$ = new Subject<string>();
  private destroyRef = inject(DestroyRef);

  private readonly statusLabels: Record<StatusKey, string> = {
    active: 'ساري',
    scheduled: 'مجدول',
    expired: 'منتهي'
  };

  private readonly statusClasses: Record<StatusKey, string> = {
    active: 'bg-green-100 text-green-700',
    scheduled: 'bg-blue-100 text-blue-700',
    expired: 'bg-gray-100 text-gray-500'
  };

  constructor(
    private fb: FormBuilder,
    private discountService: DiscountService,
    private productService: ProductService,
    public categoryService: CategoryService,
    private toastService: ToastService
  ) {
    this.form = this.fb.group(
      {
        targetType: ['Product', Validators.required],
        targetId: [null, Validators.required],
        discountType: ['Percentage', Validators.required],
        discountAmount: [null, [Validators.required, Validators.min(0.01)]],
        startDate: ['', Validators.required],
        endDate: ['', Validators.required]
      },
      { validators: discountValidator }
    );
  }

  ngOnInit(): void {
    this.loadDiscounts();
    this.categoryService.loadTree();

    this.productSearch$
      .pipe(
        map(value => value.trim()),
        debounceTime(300),
        distinctUntilChanged(),
        switchMap(term =>
          this.productService
            .getMine({ searchTerm: term, pageNumber: 1, pageSize: 6, sortBy: 'newest' })
            .pipe(catchError(() => of(null)))
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(response => {
        this.isSearchingProducts.set(false);
        this.productResults.set(response?.data.items ?? []);
      });

    this.productSearch$.next('');
  }

  // ---- الحالة ----
  statusOf(discount: Discount): StatusKey {
    const now = Date.now();
    if (now < new Date(discount.startDate).getTime()) return 'scheduled';
    if (now > new Date(discount.endDate).getTime()) return 'expired';
    return 'active';
  }

  statusLabel(discount: Discount): string {
    return this.statusLabels[this.statusOf(discount)];
  }

  statusClass(discount: Discount): string {
    return this.statusClasses[this.statusOf(discount)];
  }

  countFor(key: FilterKey): number {
    return key === 'all'
      ? this.discounts().length
      : this.discounts().filter(d => this.statusOf(d) === key).length;
  }

  private loadDiscounts(): void {
    this.isLoading.set(true);

    this.discountService.getMine().subscribe({
      next: (response) => {
        this.discounts.set(response.data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  // ---- النافذة ----
  openCreate(): void {
    const now = new Date();
    const weekLater = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);

    this.form.reset({
      targetType: 'Product',
      targetId: null,
      discountType: 'Percentage',
      discountAmount: null,
      startDate: this.toLocalInput(now),
      endDate: this.toLocalInput(weekLater)
    });

    this.selectedProduct.set(null);
    this.productQuery.set('');
    this.productSearch$.next('');
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  setTargetType(type: DiscountTargetType): void {
    this.form.patchValue({ targetType: type, targetId: null });
    this.selectedProduct.set(null);
  }

  setDiscountType(type: DiscountType): void {
    this.form.patchValue({ discountType: type });
  }

  onProductSearch(value: string): void {
    this.productQuery.set(value);
    this.isSearchingProducts.set(true);
    this.productSearch$.next(value);
  }

  selectProduct(product: Product): void {
    this.selectedProduct.set(product);
    this.form.patchValue({ targetId: product.id });
  }

  clearProduct(): void {
    this.selectedProduct.set(null);
    this.form.patchValue({ targetId: null });
  }

  // السعر بعد الخصم (معاينة سريعة للبائع لما يختار منتج)
  previewPrice(): number | null {
    const product = this.selectedProduct();
    const amount = Number(this.form.get('discountAmount')?.value);
    if (!product || !amount || amount <= 0) return null;

    const type = this.form.get('discountType')?.value as DiscountType;
    const result = type === 'Percentage' ? product.price * (1 - amount / 100) : product.price - amount;

    return result > 0 ? Math.round(result * 100) / 100 : null;
  }

  fixedTooHigh(): boolean {
    const product = this.selectedProduct();
    const amount = Number(this.form.get('discountAmount')?.value);
    return !!product && this.form.get('discountType')?.value === 'FixedAmount' && amount >= product.price;
  }

  save(): void {
    if (this.form.invalid || this.fixedTooHigh()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();

    // datetime-local بيرجّع وقت المستخدم المحلي، فبنحوله لـ UTC قبل الإرسال
    const body: CreateDiscountRequest = {
      discountType: value.discountType,
      discountAmount: Number(value.discountAmount),
      startDate: new Date(value.startDate).toISOString(),
      endDate: new Date(value.endDate).toISOString(),
      targetType: value.targetType,
      targetId: value.targetId
    };

    this.isSaving.set(true);

    this.discountService.create(body).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.isModalOpen.set(false);
        this.filter.set('all');
        this.toastService.show('تم إنشاء الخصم بنجاح', 'success');
        this.loadDiscounts();
        this.productService.loadOffersCount();
      },
      error: () => this.isSaving.set(false)
    });
  }

  delete(discount: Discount): void {
    if (!confirm(`هل تريد حذف الخصم على "${discount.targetName}"؟`)) return;

    this.deletingId.set(discount.id);

    this.discountService.delete(discount.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.toastService.show('تم حذف الخصم بنجاح', 'success');
        this.loadDiscounts();
        this.productService.loadOffersCount();
      },
      error: () => this.deletingId.set(null)
    });
  }

  private toLocalInput(date: Date): string {
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
  }
}