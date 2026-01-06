import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatMenuModule } from '@angular/material/menu';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService, Notification } from '../../core/services/notification.service';
import { Subscription, interval, startWith, switchMap } from 'rxjs';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatBadgeModule,
    MatMenuModule
  ],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css']
})
export class Navbar implements OnInit, OnDestroy {
  user$;
  notifications: Notification[] = [];
  unreadCount = 0;
  private sub: Subscription | null = null;

  constructor(
    public auth: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {
    this.user$ = this.auth.user$;
  }

  ngOnInit() {
  
    this.sub = this.auth.user$.pipe(
      switchMap(user => {
        if (user) {
          return interval(5000).pipe(
            startWith(0),
            switchMap(() => this.notificationService.getMyNotifications())
          );
        }
        return [];
      })
    ).subscribe(notifications => {

      const sorted = (notifications || []).sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
      this.notifications = sorted.slice(0, 5);
      this.unreadCount = this.notifications.filter(n => !n.isRead).length;
    });
  }

  ngOnDestroy() {
    if (this.sub) this.sub.unsubscribe();
  }

  get currentRole(): string | null {
    return this.auth.currentRole;
  }

  get currentEmail(): string | null {
    const user = this.auth.currentUser;
    return user ? (user['email'] || user['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress']) : null;
  }

  get currentName(): string | null {
    const user = this.auth.currentUser;
    return user ? (user['unique_name'] || user['name'] || user['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 'User') : 'User';
  }

  logout() {
    this.auth.logout();
  }

  isAuthPage(): boolean {
    return this.router.url.includes('/auth');
  }

  markAsRead(notification: Notification) {
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.notificationId).subscribe(() => {
        notification.isRead = true;
        this.unreadCount = this.notifications.filter(n => !n.isRead).length;
      });
    }
  }

  onMenuOpened() {
    if (this.unreadCount > 0) {

      this.unreadCount = 0;
      this.notifications.forEach(n => n.isRead = true);

      this.notificationService.markAllAsRead().subscribe({
        error: () => {
          console.error('Failed to mark all as read');
        }
      });
    }
  }
}
