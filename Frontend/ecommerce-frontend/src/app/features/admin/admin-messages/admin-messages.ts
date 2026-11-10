import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, Subscription, debounceTime, distinctUntilChanged, map } from 'rxjs';
import { LucideAngularModule, Search, X, Mail, Trash2, CheckCheck, MailOpen, MailCheck } from 'lucide-angular';
import { AdminMessageService } from '../../../core/services/admin-message';
import { ToastService } from '../../../core/services/toast';
import { ContactMessageItem } from '../../../core/models/contact-message.model';
import { TimeAgoPipe } from '../../../shared/pipes/time-ago.pipe';

type MessageFilter = 'all' | 'unread';

@Component({
  selector: 'app-admin-messages',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, TimeAgoPipe],
  templateUrl: './admin-messages.html',
  styleUrl: './admin-messages.css'
})
export class AdminMessages implements OnInit {
  readonly SearchIcon = Search;
  readonly CloseIcon = X;
  readonly ReplyIcon = Mail;
  readonly TrashIcon = Trash2;
  readonly CheckAllIcon = CheckCheck;
  readonly UnreadIcon = MailOpen;
  readonly ReadIcon = MailCheck;

  items = signal<ContactMessageItem[]>([]);
  isLoading = signal<boolean>(true);
  totalCount = signal<number>(0);

  filter = signal<MessageFilter>('all');
  searchTerm = signal<string>('');
  expandedId = signal<number | null>(null);
  busyId = signal<number | null>(null);
  isMarkingAll = signal<boolean>(false);

  currentPage = signal<number>(1);
  totalPages = signal<number>(1);
  hasPreviousPage = signal<boolean>(false);
  hasNextPage = signal<boolean>(false);

  private readonly pageSize = 10;
  private search$ = new Subject<string>();
  private request?: Subscription;
  private destroyRef = inject(DestroyRef);

  constructor(
    public messageService: AdminMessageService,
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
    this.messageService.refreshUnread();
  }

  setFilter(filter: MessageFilter): void {
    if (this.filter() === filter) return;

    this.filter.set(filter);
    this.currentPage.set(1);
    this.expandedId.set(null);
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
    this.expandedId.set(null);
    this.load();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  // فتح الرسالة بيعلّمها مقروءة، وفتحها تاني بيقفلها
  toggle(message: ContactMessageItem): void {
    if (this.expandedId() === message.id) {
      this.expandedId.set(null);
      return;
    }

    this.expandedId.set(message.id);

    if (!message.isRead) {
      this.setLocalRead(message.id, true);
      this.messageService.markRead(message.id).subscribe({
        error: () => this.setLocalRead(message.id, false)
      });
    }
  }

  toggleRead(message: ContactMessageItem): void {
    const makeRead = !message.isRead;
    this.busyId.set(message.id);

    const request = makeRead
      ? this.messageService.markRead(message.id)
      : this.messageService.markUnread(message.id);

    request.subscribe({
      next: () => {
        this.busyId.set(null);
        this.setLocalRead(message.id, makeRead);
      },
      error: () => this.busyId.set(null)
    });
  }

  markAllRead(): void {
    this.isMarkingAll.set(true);

    this.messageService.markAllRead().subscribe({
      next: () => {
        this.isMarkingAll.set(false);
        this.items.update(list => list.map(m => ({ ...m, isRead: true })));
        this.toastService.show('تم تحديد كل الرسائل كمقروءة', 'success');
      },
      error: () => this.isMarkingAll.set(false)
    });
  }

  remove(message: ContactMessageItem): void {
    if (!confirm(`هل تريد حذف رسالة "${message.name}" نهائيًا؟`)) return;

    this.busyId.set(message.id);

    this.messageService.delete(message.id).subscribe({
      next: () => {
        this.busyId.set(null);
        this.expandedId.set(null);
        this.toastService.show('تم حذف الرسالة', 'success');
        this.load(true);
      },
      error: () => this.busyId.set(null)
    });
  }

  // رابط "رد بالإيميل": بيفتح برنامج الإيميل برسالة جاهزة لعنوان العميل
  replyHref(message: ContactMessageItem): string {
    return `mailto:${message.email}?subject=${encodeURIComponent('Re: ' + message.subject)}`;
  }

  private setLocalRead(id: number, isRead: boolean): void {
    this.items.update(list => list.map(m => (m.id === id ? { ...m, isRead } : m)));
  }

  private load(silent = false): void {
    this.request?.unsubscribe(); // نلغي أي طلب قديم لسه شغال
    if (!silent) this.isLoading.set(true);

    this.request = this.messageService.getAll({
      filter: this.filter(),
      search: this.searchTerm().trim(),
      pageNumber: this.currentPage(),
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        const result = response.data;

        // لو الصفحة فضيت بعد الحذف وفي صفحات قبلها، نرجع صفحة لورا
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