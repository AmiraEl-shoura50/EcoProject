import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { AppNotification } from '../models/notification.model';
import { PaginatedResult } from '../models/product.model';
import { ToastService } from './toast';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = `${environment.apiUrl}/notifications`;
  // الـ Hub على /hubs مش /api، فبنشيل /api من نهاية الرابط
  private hubUrl = `${environment.apiUrl.replace(/\/api\/?$/, '')}/hubs/notifications`;

  // آخر الإشعارات (للقائمة المنسدلة) وعدد غير المقروء (للعلامة فوق الجرس)
  latest = signal<AppNotification[]>([]);
  unreadCount = signal<number>(0);

  private connection: HubConnection | null = null;

  constructor(
    private http: HttpClient,
    private toastService: ToastService
  ) {}

  // بتتنادى بعد تسجيل الدخول وعند فتح الموقع لو المستخدم مسجل (آمنة لو اتنادت أكتر من مرة)
  start(): void {
    if (this.connection) {
      // الاتصال موجود لكن ممكن يكون فشل في أول محاولة (مثلًا التوكن كان منتهي)، فنحاول تاني بالتوكن الحالي
      if (this.connection.state === HubConnectionState.Disconnected) this.tryStart();
      return;
    }

    this.refresh();
    this.connect();
  }

  stop(): void {
    this.connection?.stop();
    this.connection = null;
    this.latest.set([]);
    this.unreadCount.set(0);
  }

  getPage(pageNumber: number, pageSize: number): Observable<ApiResponse<PaginatedResult<AppNotification>>> {
    return this.http.get<ApiResponse<PaginatedResult<AppNotification>>>(this.apiUrl, {
      params: { pageNumber, pageSize }
    });
  }

  markAsRead(notification: AppNotification): Observable<unknown> {
    return this.http.put(`${this.apiUrl}/${notification.id}/read`, {}).pipe(
      tap(() => {
        if (!notification.isRead) this.unreadCount.update(count => Math.max(0, count - 1));
        this.latest.update(list => list.map(n => (n.id === notification.id ? { ...n, isRead: true } : n)));
      })
    );
  }

  markAllAsRead(): Observable<unknown> {
    return this.http.put(`${this.apiUrl}/read-all`, {}).pipe(
      tap(() => {
        this.unreadCount.set(0);
        this.latest.update(list => list.map(n => ({ ...n, isRead: true })));
      })
    );
  }

  delete(notification: AppNotification): Observable<unknown> {
    return this.http.delete(`${this.apiUrl}/${notification.id}`).pipe(
      tap(() => {
        if (!notification.isRead) this.unreadCount.update(count => Math.max(0, count - 1));
        this.latest.update(list => list.filter(n => n.id !== notification.id));
        this.refresh(); // القائمة المنسدلة بتعرض آخر 8، فنملاها تاني بعد الحذف
      })
    );
  }

  deleteRead(): Observable<unknown> {
    return this.http.delete(`${this.apiUrl}/read`).pipe(
      tap(() => {
        this.latest.update(list => list.filter(n => !n.isRead));
        this.refresh();
      })
    );
  }

    // الصفحة اللي يفتحها الإشعار: الرابط لو موجود، وإلا حسب دور المستخدم
  targetFor(notification: AppNotification, role: string | null): string {
    if (notification.link) return notification.link;
    if (notification.title.includes('عرض')) return '/offers';
    return role === 'Seller' ? '/seller/orders' : '/orders';
  }

  private refresh(): void {
    this.http
      .get<ApiResponse<PaginatedResult<AppNotification>>>(this.apiUrl, { params: { pageNumber: 1, pageSize: 8 } })
      .subscribe({ next: (response) => this.latest.set(response.data.items) });

    this.http
      .get<ApiResponse<{ count: number }>>(`${this.apiUrl}/unread-count`)
      .subscribe({ next: (response) => this.unreadCount.set(response.data.count) });
  }

  private connect(): void {
    const connection = new HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        // بيقرا التوكن الحالي كل مرة، فبعد تجديده الاتصال بيستخدم الجديد
        accessTokenFactory: () => localStorage.getItem('accessToken') ?? ''
      })
      .withAutomaticReconnect()
      .build();

    connection.on('ReceiveNotification', (notification: AppNotification) => {
      this.latest.update(list => [notification, ...list].slice(0, 8));
      this.unreadCount.update(count => count + 1);
      this.toastService.show(notification.title, 'info');
    });

    // لو الاتصال رجع بعد انقطاع، نجيب الإشعارات اللي فاتتنا
    connection.onreconnected(() => this.refresh());

    this.connection = connection;
    this.tryStart();
  }

  // بيحاول يفتح الاتصال، ولو فشل بيعيد 4 مرات بفاصل متزايد (الباك ممكن يكون لسه بيشتغل أو التوكن بيتجدد)
  private tryStart(attempt = 1): void {
    const connection = this.connection;
    if (!connection || connection.state !== HubConnectionState.Disconnected) return;

    connection.start().catch(() => {
      if (attempt >= 5) return;

      setTimeout(() => {
        if (this.connection === connection) this.tryStart(attempt + 1);
      }, attempt * 3000);
    });
  }
}