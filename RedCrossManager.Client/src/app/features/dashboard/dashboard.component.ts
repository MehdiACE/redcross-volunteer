import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DashboardService } from '../../core/services/dashboard.service';
import { NotificationService } from '../../core/services/notification.service';
import { VolunteerDashboardDto } from '../../core/models/dashboard.model';
import { NotificationItem } from '../../core/models/notification.model';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatProgressSpinnerModule, TranslateModule, RouterLink],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit, OnDestroy {
  dashboard: VolunteerDashboardDto | null = null;
  notifications: NotificationItem[] = [];
  isLoading = false;
  loadError = false;
  notificationsLoading = false;
  notificationsError = false;
  private destroy$ = new Subject<void>();

  constructor(
    private dashboardService: DashboardService,
    private notificationService: NotificationService,
    private authService: AuthService
  ) {}

  get isAdmin(): boolean {
    return this.authService.hasRole('Admin');
  }

  ngOnInit(): void {
    this.fetchDashboard();
    this.loadNotifications();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private fetchDashboard(): void {
    this.isLoading = true;
    this.loadError = false;

    this.dashboardService.getDashboard()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (dashboard) => {
          this.dashboard = dashboard;
          this.isLoading = false;
        },
        error: () => {
          this.loadError = true;
          this.isLoading = false;
        }
      });
  }

  private loadNotifications(): void {
    this.notificationsLoading = true;
    this.notificationsError = false;

    this.notificationService.getMyNotifications()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (notifications) => {
          this.notifications = notifications;
          this.notificationsLoading = false;
        },
        error: () => {
          this.notificationsError = true;
          this.notificationsLoading = false;
        }
      });
  }

  markAsRead(notificationId: string): void {
    this.notificationService.markAsRead(notificationId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          const notification = this.notifications.find(n => n.id === notificationId);
          if (notification) {
            notification.isRead = true;
          }
        },
        error: (err) => {
          console.error('Failed to mark notification as read', err);
        }
      });
  }
}
