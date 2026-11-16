import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, Subscription, debounceTime, distinctUntilChanged, map } from 'rxjs';
import { LucideAngularModule, Search, X, Eye, EyeOff } from 'lucide-angular';
import { AdminProductService } from '../../../core/services/admin-product';
import { ToastService } from '../../../core/services/toast';
import { AdminProduct } from '../../../core/models/admin-product.model';

type VisibilityFilter = 'all' | 'hidden' | 'visible';

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [CommonModule, RouterLink, LucideAngularModule],
  templateUrl: './admin-products.html',
  styleUrl: './admin-products.css'
})
export class AdminProducts implements OnInit {
  readonly SearchIcon = Search;
  readonly CloseIcon = X;
  readonly EyeIcon = Eye;
  readonly EyeOffIcon = EyeOff;

  readonly filters: { key: VisibilityFilter; label: string }[] = [
    { key: 'all', label: 'الكل' },
    { key: 'visible', label: 'الظاهرة' },
    { key: 'hidden', label: 'المخفية' }
  ];

  items = signal<AdminProduct[]>([]);
  isLoading = signal<boolean>(true);
  totalCount = signal<number>(0);
  togglingId = signal<number | null>(null);

  filter = signal<VisibilityFilter>('all');
  searchTerm = signal<string>('');

  currentPage = signal<number>(1);
  totalPages = signal<number>(1);
  hasPreviousPage = signal<boolean>(false);
  hasNextPage = signal<boolean>(false);

  private readonly pageSize = 10;
  private search$ = new Subject<string>();
  private request?: Subscription;
  private destroyRef = inject(DestroyRef);

  constructor(
    private adminProductService: AdminProductService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    // بحث مباشر بعد 300ms من آخر حرف
    this.search$
      .pipe(
        map(value => value.trim()),
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.currentPage.set(1);
        this.load();
      });

    this.load();
  }

  setFilter(filter: VisibilityFilter): void {
    if (this.filter() === filter) return;

    this.filter.set(filter);
    this.currentPage.set(1);
    this.load();
  }

  onSearchInput(value: string): void {
    this.searchTerm.set(value);
    this.search$.next(value);
  }

  clearSearch(): void {
    this.searchTerm.set('');
    this.search$.next('');
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;

    this.currentPage.set(page);
    this.load();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  toggleVisibility(product: AdminProduct): void {
    const hide = !product.isHiddenByAdmin;

    const question = hide
      ? `هل تريد إخفاء "${product.name}" عن العملاء؟ البائع لن يقدر يظهره، وهيوصله إشعار.`
      : `هل تريد إلغاء إخفاء الإدارة عن "${product.name}"؟`;

    if (!confirm(question)) return;

    this.togglingId.set(product.id);

    this.adminProductService.setVisibility(product.id, hide).subscribe({
      next: () => {
        this.togglingId.set(null);
        this.toastService.show(hide ? 'تم إخفاء المنتج عن العملاء' : 'تم إلغاء إخفاء الإدارة للمنتج', 'success');
        this.items.update(list => list.map(p => (p.id === product.id ? { ...p, isHiddenByAdmin: hide } : p)));

        if (this.filter() !== 'all') this.load(true); // ممكن المنتج يخرج من الفلتر
      },
      error: () => this.togglingId.set(null)
    });
  }

  private load(silent = false): void {
    this.request?.unsubscribe(); // نلغي أي طلب قديم لسه شغال
    if (!silent) this.isLoading.set(true);

    this.request = this.adminProductService.getAll({
      searchTerm: this.searchTerm().trim(),
      visibilityFilter: this.filter() === 'all' ? '' : this.filter(),
      pageNumber: this.currentPage(),
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        const result = response.data;

        // لو الصفحة فضيت وفي صفحات قبلها، نرجع صفحة لورا
        if (result.items.length === 0 && this.currentPage() > 1) {
          this.currentPage.set(this.currentPage() - 1);
          this.load(silent);
          return;
        }

        this.items.set(result.items);
        this.totalCount.set(result.totalCount);
        this.totalPages.set(result.totalPages);
        this.hasPreviousPage.set(result.hasPreviousPage);
        this.hasNextPage.set(result.hasNextPage);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}