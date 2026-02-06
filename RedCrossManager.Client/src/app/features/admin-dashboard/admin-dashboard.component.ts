import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import { AgGridAngular } from 'ag-grid-angular';
import { ClientSideRowModelModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { Observable, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AdminDashboardService } from '../../core/services/admin-dashboard.service';
import { AdminOnboardingStep, AdminVolunteerListItem } from '../../core/models/admin-dashboard.model';
import { NotificationItem } from '../../core/models/notification.model';
import { MessageItem } from '../../core/models/message.model';
import { NotificationService } from '../../core/services/notification.service';
import { MessageService } from '../../core/services/message.service';
import { AgGridThemeService } from '../../core/services/ag-grid-theme.service';

ModuleRegistry.registerModules([ClientSideRowModelModule]);

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatProgressSpinnerModule, TranslateModule, AgGridAngular],
  templateUrl: './admin-dashboard.component.html'
})
export class AdminDashboardComponent implements OnInit, OnDestroy {
  volunteers: AdminVolunteerListItem[] = [];
  pendingSteps: AdminOnboardingStep[] = [];
  notifications: NotificationItem[] = [];
  messages: MessageItem[] = [];
  newMessageContent = '';
  selectedVolunteerId = '';
  isLoadingVolunteers = false;
  isLoadingSteps = false;
  isLoadingNotifications = false;
  isLoadingMessages = false;
  volunteerError = false;
  stepsError = false;
  notificationsError = false;
  messagesError = false;
  agGridThemeClass$!: Observable<string>;

  colDefs: ColDef[] = [
    {
      headerName: 'Name',
      valueGetter: params => `${params.data?.firstName ?? ''} ${params.data?.lastName ?? ''}`.trim(),
      flex: 1,
      minWidth: 180
    },
    { field: 'email', headerName: 'Email', flex: 1, minWidth: 220 },
    { field: 'status', headerName: 'Status', minWidth: 120 },
    { field: 'registeredAt', headerName: 'Registered', minWidth: 140 },
    { field: 'languagePreference', headerName: 'Lang', minWidth: 90 },
    { field: 'isMinor', headerName: 'Minor', minWidth: 90 }
  ];

  defaultColDef: ColDef = {
    sortable: true,
    filter: true,
    resizable: true
  };

  private destroy$ = new Subject<void>();

  constructor(
    private adminDashboardService: AdminDashboardService,
    private notificationService: NotificationService,
    private messageService: MessageService,
    private agGridThemeService: AgGridThemeService
  ) {
    this.agGridThemeClass$ = this.agGridThemeService.themeClass$;
  }

  ngOnInit(): void {
    this.loadVolunteers();
    this.loadPendingSteps();
    this.loadNotifications();
    this.loadInbox();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  approveStep(step: AdminOnboardingStep): void {
    this.adminDashboardService.reviewStep(step.id, { approved: true })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => this.loadPendingSteps(),
        error: () => {
          this.stepsError = true;
        }
      });
  }

  rejectStep(step: AdminOnboardingStep): void {
    this.adminDashboardService.reviewStep(step.id, { approved: false })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => this.loadPendingSteps(),
        error: () => {
          this.stepsError = true;
        }
      });
  }

  sendMessage(): void {
    if (!this.newMessageContent.trim() || !this.selectedVolunteerId) return;

    this.messageService.sendMessage({
      toVolunteerId: this.selectedVolunteerId,
      content: this.newMessageContent
    }).pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.newMessageContent = '';
          this.selectedVolunteerId = '';
          this.loadInbox();
        },
        error: () => {
          this.messagesError = true;
        }
      });
  }

  markMessageAsRead(message: MessageItem): void {
    if (message.isRead) return;
    this.messageService.markAsRead(message.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          message.isRead = true;
        }
      });
  }

  private loadVolunteers(): void {
    this.isLoadingVolunteers = true;
    this.volunteerError = false;
    this.adminDashboardService.getVolunteers()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: data => {
          this.volunteers = data;
          this.isLoadingVolunteers = false;
        },
        error: () => {
          this.volunteerError = true;
          this.isLoadingVolunteers = false;
        }
      });
  }

  private loadPendingSteps(): void {
    this.isLoadingSteps = true;
    this.stepsError = false;
    this.adminDashboardService.getPendingOnboardingSteps()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: data => {
          this.pendingSteps = data;
          this.isLoadingSteps = false;
        },
        error: () => {
          this.stepsError = true;
          this.isLoadingSteps = false;
        }
      });
  }

  private loadNotifications(): void {
    this.isLoadingNotifications = true;
    this.notificationsError = false;
    this.notificationService.getMyNotifications()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: data => {
          this.notifications = data;
          this.isLoadingNotifications = false;
        },
        error: () => {
          this.notificationsError = true;
          this.isLoadingNotifications = false;
        }
      });
  }

  private loadInbox(): void {
    this.isLoadingMessages = true;
    this.messagesError = false;
    this.messageService.getInbox()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: data => {
          this.messages = data;
          this.isLoadingMessages = false;
        },
        error: () => {
          this.messagesError = true;
          this.isLoadingMessages = false;
        }
      });
  }
}
