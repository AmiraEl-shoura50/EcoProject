import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, Subscription, debounceTime, distinctUntilChanged, map } from 'rxjs';
import { LucideAngularModule, Search, X } from 'lucide-angular';
import { AdminService } from '../../../core/services/admin';
import { ToastService } from '../../../core/services/toast';
import { AdminUser } from '../../../core/models/admin.model';
import { RouterLink, ActivatedRoute } from '@angular/router';
@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, RouterLink],
  templateUrl: './admin-users.html',
  styleUrl: './admin-users.css'
})
export class AdminUsers implements OnInit {
  readonly SearchIcon = Search;
  readonly CloseIcon = X;

  users = signal<AdminUser[]>([]);
  isLoading = signal<boolean>(true);
  busyId = signal<string | null>(null);

  searchTerm = signal<string>('');
  roleFilter = signal<string>('all');
  totalCount = signal<number>(0);

  currentPage = signal<number>(1);
  totalPages = signal<number>(1);
  hasPreviousPage = signal<boolean>(false);
  hasNextPage = signal<boolean>(false);

  roleFilters = [
    { key: 'all', label: 'الكل' },
    { key: 'Customer', label: 'العملاء' },
    { key: 'Seller', label: 'البائعون' },
    { key: 'Admin', label: 'المدراء' }
  ];

  private readonly roleLabels: Record<string, string> = {
    Customer: 'عميل',
    Seller: 'بائع',
    Admin: 'مدير'
  };

  private readonly roleClasses: Record<string, string> = {
    Customer: 'bg-blue-100 text-blue-700',
    Seller: 'bg-purple-100 text-purple-700',
    Admin: 'bg-yellow-100 text-yellow-700'
  };

  private readonly pageSize = 10;
  private search$ = new Subject<string>();
  private request?: Subscription;
  private destroyRef = inject(DestroyRef);

  constructor(
    private adminService: AdminService,
    private toastService: ToastService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
   const role = this.route.snapshot.queryParamMap.get('role');
  if (role && this.roleFilters.some(f => f.key === role)) {
    this.roleFilter.set(role);
  }

  this.loadUsers();
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
        this.loadUsers();
      });
  }

  onSearchInput(value: string): void {
    this.searchTerm.set(value);
    this.search$.next(value);
  }

  clearSearch(): void {
    this.searchTerm.set('');
    this.search$.next('');
  }

  setRoleFilter(key: string): void {
    this.roleFilter.set(key);
    this.currentPage.set(1);
    this.loadUsers();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.currentPage.set(page);
    this.loadUsers();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  roleLabel(role: string): string {
    return this.roleLabels[role] ?? role;
  }

  roleClass(role: string): string {
    return this.roleClasses[role] ?? 'bg-gray-100 text-gray-600';
  }

  initial(user: AdminUser): string {
    return (user.firstName || user.email).charAt(0);
  }

  isAdmin(user: AdminUser): boolean {
    return user.roles.includes('Admin');
  }

  toggleStatus(user: AdminUser): void {
    const name = `${user.firstName} ${user.lastName}`.trim();
    const question = user.isActive
      ? `هل تريد إيقاف حساب "${name}"؟ لن يتمكن من تسجيل الدخول حتى يتم تفعيله.`
      : `هل تريد إعادة تفعيل حساب "${name}"؟`;

    if (!confirm(question)) return;

    const newStatus = !user.isActive;
    this.busyId.set(user.id);

    this.adminService.setUserStatus(user.id, newStatus).subscribe({
      next: () => {
        this.busyId.set(null);

        // نحدّث المستخدم في القائمة مباشرة من غير إعادة تحميل
        this.users.update(list => list.map(u => (u.id === user.id ? { ...u, isActive: newStatus } : u)));

        this.toastService.show(newStatus ? 'تم تفعيل الحساب' : 'تم إيقاف الحساب', 'success');
      },
      error: () => this.busyId.set(null)
    });
  }

  private loadUsers(): void {
    this.request?.unsubscribe(); // نلغي أي طلب قديم لسه شغال
    this.isLoading.set(true);

    const role = this.roleFilter();

    this.request = this.adminService.getUsers({
      role: role === 'all' ? undefined : role,
      search: this.searchTerm().trim() || undefined,
      pageNumber: this.currentPage(),
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        const result = response.data;
        this.users.set(result.items);
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