import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { LucideAngularModule, Bell, CheckCheck, Trash2 } from 'lucide-angular';
import { NotificationService } from '../../../core/services/notification';
import { AuthService } from '../../../core/services/auth';
import { ToastService } from '../../../core/services/toast';
import { AppNotification } from '../../../core/models/notification.model';
import { TimeAgoPipe } from '../../../shared/pipes/time-ago.pipe';

@Component({
  selector: 'app-notifications-page',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, TimeAgoPipe],
  templateUrl: './notifications-page.html',
  styleUrl: './notifications-page.css'
})
export class NotificationsPage implements OnInit {
  readonly BellIcon = Bell;
  readonly CheckAllIcon = CheckCheck;
  readonly TrashIcon = Trash2;

  items = signal<AppNotification[]>([]);
  isLoading = signal<boolean>(true);
  totalCount = signal<number>(0);

  currentPage = signal<number>(1);
  totalPages = signal<number>(1);
  hasPreviousPage = signal<boolean>(false);
  hasNextPage = signal<boolean>(false);

  private readonly pageSize = 10;

  constructor(
    public notificationService: NotificationService,
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.load();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;

    this.currentPage.set(page);
    this.load();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  open(notification: AppNotification): void {
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification).subscribe();
      this.items.update(list => list.map(n => (n.id === notification.id ? { ...n, isRead: true } : n)));
    }

    this.router.navigateByUrl(this.notificationService.targetFor(notification, this.authService.userRole()));
  }

  markAllRead(): void {
    this.notificationService.markAllAsRead().subscribe({
      next: () => this.items.update(list => list.map(n => ({ ...n, isRead: true })))
    });
  }

  remove(notification: AppNotification): void {
    this.notificationService.delete(notification).subscribe({
      next: () => {
        this.items.update(list => list.filter(n => n.id !== notification.id));
        this.load(true); // إعادة تحميل بدون وميض عشان العدد والترقيم يتظبطوا
      }
    });
  }

  clearRead(): void {
    if (!confirm('هل تريد حذف كل الإشعارات المقروءة؟')) return;

    this.notificationService.deleteRead().subscribe({
      next: () => {
        this.toastService.show('تم حذف الإشعارات المقروءة', 'success');
        this.currentPage.set(1);
        this.load();
      }
    });
  }

  private load(silent = false): void {
    if (!silent) this.isLoading.set(true);

    this.notificationService.getPage(this.currentPage(), this.pageSize).subscribe({
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