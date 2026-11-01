import { Injectable, signal } from '@angular/core';

interface BeforeInstallPromptEvent extends Event {
  prompt(): Promise<void>;
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>;
}

@Injectable({
  providedIn: 'root'
})
export class PwaInstallService {
  // المتصفح عرض خيار التثبيت وفي انتظار ضغطة المستخدم
  canInstall = signal<boolean>(false);
  // الموقع مفتوح كتطبيق مثبّت
  isInstalled = signal<boolean>(window.matchMedia('(display-mode: standalone)').matches);

  private deferredPrompt: BeforeInstallPromptEvent | null = null;

  constructor() {
    window.addEventListener('beforeinstallprompt', (event) => {
      event.preventDefault(); // نمنع الشريط التلقائي، ونعرض زرارنا في المكان المناسب
      this.deferredPrompt = event as BeforeInstallPromptEvent;
      this.canInstall.set(true);
    });

    window.addEventListener('appinstalled', () => {
      this.deferredPrompt = null;
      this.canInstall.set(false);
      this.isInstalled.set(true);
    });
  }

  async install(): Promise<void> {
    if (!this.deferredPrompt) return;

    await this.deferredPrompt.prompt();
    await this.deferredPrompt.userChoice;

    this.deferredPrompt = null;
    this.canInstall.set(false);
  }
}