import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, Subscription, debounceTime, distinctUntilChanged, map } from 'rxjs';
import { LucideAngularModule, Plus, Pencil, Trash2, X, Search, ImagePlus } from 'lucide-angular';
import { ProductService } from '../../../core/services/product';
import { CategoryService } from '../../../core/services/category';
import { ToastService } from '../../../core/services/toast';
import { Product, StockSummary } from '../../../core/models/product.model';
import { Category } from '../../../core/models/category.model';

type StockFilter = 'all' | 'attention' | 'low' | 'out';

interface StockTab {
  key: StockFilter;
  label: string;
  count: number | null;
}

const STOCK_FILTERS: StockFilter[] = ['all', 'attention', 'low', 'out'];

@Component({
  selector: 'app-seller-products',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LucideAngularModule],
  templateUrl: './seller-products.html',
  styleUrl: './seller-products.css'
})
export class SellerProducts implements OnInit {
  readonly PlusIcon = Plus;
  readonly EditIcon = Pencil;
  readonly TrashIcon = Trash2;
  readonly CloseIcon = X;
  readonly SearchIcon = Search;
  readonly ImageIcon = ImagePlus;

  products = signal<Product[]>([]);
  categories = signal<Category[]>([]);
  isLoading = signal<boolean>(true);
  deletingId = signal<number | null>(null);

  // ---- البحث المباشر ----
  searchTerm = signal<string>('');
  private search$ = new Subject<string>();

  // ---- فلتر المخزون ----
  stockFilter = signal<StockFilter>('all');
  stockSummary = signal<StockSummary | null>(null);

  stockTabs = computed<StockTab[]>(() => {
    const summary = this.stockSummary();

    return [
      { key: 'all', label: 'كل المنتجات', count: null },
      { key: 'attention', label: 'يحتاج انتباه', count: summary ? summary.lowStock + summary.outOfStock : null },
      { key: 'low', label: 'قاربت على النفاد', count: summary ? summary.lowStock : null },
      { key: 'out', label: 'نفذت', count: summary ? summary.outOfStock : null }
    ];
  });

  // ---- التحديد والحذف ----
  selectedIds = signal<number[]>([]);
  isDeletingMany = signal<boolean>(false);

  allSelected = computed(() =>
    this.products().length > 0 &&
    this.products().every(p => this.selectedIds().includes(p.id))
  );

  currentPage = signal<number>(1);
  totalPages = signal<number>(1);
  hasPreviousPage = signal<boolean>(false);
  hasNextPage = signal<boolean>(false);
  totalCount = signal<number>(0);

  // ---- النافذة (إضافة / تعديل) ----
  isModalOpen = signal<boolean>(false);
  editingProduct = signal<Product | null>(null);
  selectedFile = signal<File | null>(null);
  previewUrl = signal<string | null>(null);
  isSaving = signal<boolean>(false);

  productForm: FormGroup;

  private readonly pageSize = 8;
  private readonly maxImageSize = 5 * 1024 * 1024; // 5MB
  private productsRequest?: Subscription;
  private destroyRef = inject(DestroyRef);

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private productService: ProductService,
    private categoryService: CategoryService,
    private toastService: ToastService
  ) {
    this.productForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.required, Validators.maxLength(2000)]],
      price: [null, [Validators.required, Validators.min(0.01)]],
      stockQuantity: [0, [Validators.required, Validators.min(0), Validators.pattern(/^\d+$/)]],
      categoryId: [null, Validators.required]
    });
  }

  ngOnInit(): void {
    // لو جاي من بطاقة المخزون في النظرة العامة (?stock=attention) نفتح على الفلتر ده
    const stock = this.route.snapshot.queryParamMap.get('stock');
    if (stock && (STOCK_FILTERS as string[]).includes(stock)) {
      this.stockFilter.set(stock as StockFilter);
    }

    this.loadProducts();
    this.loadStockSummary();

    this.categoryService.getTree().subscribe({
      next: (response) => this.categories.set(response.data)
    });

    // بحث مباشر: ننتظر 300ms بعد آخر حرف، ونبحث لو النص اتغير فعلًا
    this.search$
      .pipe(
        map(value => value.trim()),
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.currentPage.set(1);
        this.selectedIds.set([]);
        this.loadProducts();
      });
  }

  // ---- البحث ----
  onSearchInput(value: string): void {
    this.searchTerm.set(value);
    this.search$.next(value);
  }

  clearSearch(): void {
    this.searchTerm.set('');
    this.search$.next('');
  }

  // ---- فلتر المخزون ----
  setStockFilter(filter: StockFilter): void {
    if (this.stockFilter() === filter) return;

    this.stockFilter.set(filter);
    this.currentPage.set(1);
    this.selectedIds.set([]);
    this.loadProducts();
  }

  // ---- التحديد ----
  isSelected(productId: number): boolean {
    return this.selectedIds().includes(productId);
  }

  toggleSelect(productId: number): void {
    this.selectedIds.update(ids =>
      ids.includes(productId) ? ids.filter(id => id !== productId) : [...ids, productId]
    );
  }

  toggleSelectAll(): void {
    this.selectedIds.set(this.allSelected() ? [] : this.products().map(p => p.id));
  }

  deleteSelected(): void {
    const ids = this.selectedIds();
    if (ids.length === 0) return;

    if (!confirm(`هل تريد حذف ${ids.length} منتج؟ لا يمكن التراجع عن هذا الإجراء.`)) return;

    this.isDeletingMany.set(true);

    this.productService.deleteMany(ids).subscribe({
      next: (response) => {
        const { deletedCount, blockedCount } = response.data;
        this.isDeletingMany.set(false);
        this.selectedIds.set([]);

        const message = blockedCount > 0
          ? `تم حذف ${deletedCount} منتج، وتعذر حذف ${blockedCount} لأنها مرتبطة بطلبات سابقة`
          : `تم حذف ${deletedCount} منتج بنجاح`;
        this.toastService.show(message, 'success');

        this.loadProducts();
        this.loadStockSummary();
      },
      error: () => this.isDeletingMany.set(false)
    });
  }

  // ---- القائمة ----
  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.currentPage.set(page);
    this.selectedIds.set([]);
    this.loadProducts();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  private loadStockSummary(): void {
    this.productService.getStockSummary().subscribe({
      next: (response) => this.stockSummary.set(response.data)
    });
  }

  private loadProducts(): void {
    this.productsRequest?.unsubscribe(); // نلغي أي بحث قديم لسه شغال
    this.isLoading.set(true);

    const filter = this.stockFilter();

    this.productsRequest = this.productService.getMine({
      searchTerm: this.searchTerm().trim(),
      pageNumber: this.currentPage(),
      pageSize: this.pageSize,
      // مع فلتر المخزون: الأقل مخزونًا الأول
      sortBy: filter === 'all' ? 'newest' : 'stock_asc',
      stockFilter: filter === 'all' ? undefined : filter
    }).subscribe({
      next: (response) => {
        const result = response.data;

        // لو مسحنا آخر منتج في الصفحة الأخيرة نرجع صفحة لورا
        if (result.items.length === 0 && this.currentPage() > 1) {
          this.currentPage.set(this.currentPage() - 1);
          this.loadProducts();
          return;
        }

        this.products.set(result.items);
        this.totalPages.set(result.totalPages);
        this.totalCount.set(result.totalCount);
        this.hasPreviousPage.set(result.hasPreviousPage);
        this.hasNextPage.set(result.hasNextPage);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  // ---- النافذة ----
  openCreate(): void {
    this.editingProduct.set(null);
    this.productForm.reset({ name: '', description: '', price: null, stockQuantity: 0, categoryId: null });
    this.selectedFile.set(null);
    this.previewUrl.set(null);
    this.isModalOpen.set(true);
  }

  openEdit(product: Product): void {
    this.editingProduct.set(product);
    this.productForm.reset({
      name: product.name,
      description: product.description,
      price: product.price,
      stockQuantity: product.stockQuantity,
      categoryId: product.categoryId
    });
    this.selectedFile.set(null);
    this.previewUrl.set(product.imageUrl);
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
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

  save(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    const value = this.productForm.getRawValue();

    // الأسماء بحرف كبير عشان تطابق CreateProductDto / UpdateProductDto
    const formData = new FormData();
    formData.append('Name', value.name.trim());
    formData.append('Description', value.description.trim());
    formData.append('Price', String(value.price));
    formData.append('StockQuantity', String(value.stockQuantity));
    formData.append('CategoryId', String(value.categoryId));

    const file = this.selectedFile();
    if (file) formData.append('Image', file);

    this.isSaving.set(true);

    const editing = this.editingProduct();
    const request = editing
      ? this.productService.update(editing.id, formData)
      : this.productService.create(formData);

    request.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.isModalOpen.set(false);
        this.toastService.show(editing ? 'تم تعديل المنتج بنجاح' : 'تمت إضافة المنتج بنجاح', 'success');

        if (!editing) this.currentPage.set(1); // الجديد بيظهر في الأول
        this.loadProducts();
        this.loadStockSummary(); // الكمية ممكن تكون اتغيّرت
      },
      error: () => this.isSaving.set(false)
    });
  }

  // ---- حذف منتج واحد ----
  delete(product: Product): void {
    if (!confirm(`هل تريد حذف المنتج "${product.name}"؟`)) return;

    this.deletingId.set(product.id);

    this.productService.delete(product.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.selectedIds.update(ids => ids.filter(id => id !== product.id));
        this.toastService.show('تم حذف المنتج بنجاح', 'success');
        this.loadProducts();
        this.loadStockSummary();
      },
      error: () => this.deletingId.set(null)
    });
  }
}