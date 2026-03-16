import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class NotificationService {

  isSupported(): boolean {
    return 'Notification' in window;
  }

  async requestPermission(): Promise<NotificationPermission> {
    if (!this.isSupported()) return 'denied';
    return Notification.requestPermission();
  }

  showNotification(title: string, body: string): void {
    if (!this.isSupported() || Notification.permission !== 'granted') return;
    new Notification(title, { body, icon: '/favicon.ico' });
  }
}
