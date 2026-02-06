import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MessageService } from '../../core/services/message.service';
import { AdminDashboardService } from '../../core/services/admin-dashboard.service';
import { AuthService } from '../../core/services/auth.service';
import { MessageItem } from '../../core/models/message.model';
import { AdminVolunteerListItem } from '../../core/models/admin-dashboard.model';
import { ReadMessageComponent } from './read-message.component';
import { ComposeMessageComponent } from './compose-message.component';

@Component({
  selector: 'app-user-message',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    MatAutocompleteModule,
    MatFormFieldModule,
    MatInputModule,
    ReadMessageComponent,
    ComposeMessageComponent
  ],
  templateUrl: './user-message.component.html'
})
export class UserMessageComponent implements OnInit, OnDestroy {
  showMessageMenu = false;
  showMessageDrawer = false;
  unreadCount = 0;
  recentMessages: MessageItem[] = [];
  selectedMessage: MessageItem | null = null;
  drawerMode: 'read' | 'compose' = 'read';
  composeContent = '';
  composeVolunteerSearch = '';
  composeVolunteerId = '';
  adminTargetUserId: string | null = null;
  volunteers: AdminVolunteerListItem[] = [];
  isAdmin = false;
  private destroy$ = new Subject<void>();

  constructor(
    private messageService: MessageService,
    private adminDashboardService: AdminDashboardService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.isAdmin = this.authService.hasRole('Admin');
    this.loadMessageSummary();
    if (this.isAdmin) {
      this.loadVolunteers();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  toggleMessageMenu(): void {
    this.showMessageMenu = !this.showMessageMenu;
    if (this.showMessageMenu) {
      this.loadMessageSummary();
    }
  }

  openMessageDrawer(mode: 'read' | 'compose', message?: MessageItem): void {
    this.drawerMode = mode;
    this.selectedMessage = message ?? null;
    this.composeContent = '';
    this.composeVolunteerSearch = '';
    this.composeVolunteerId = '';
    this.showMessageDrawer = true;
    this.showMessageMenu = false;

    if (message && !message.isRead) {
      this.messageService.markAsRead(message.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            message.isRead = true;
            this.loadMessageSummary();
          }
        });
    }

    if (mode === 'compose' && this.isAdmin) {
      this.loadVolunteers();
    }
  }

  closeMessageDrawer(): void {
    this.showMessageDrawer = false;
  }

  sendDrawerMessage(): void {
    if (!this.composeContent.trim()) return;

    if (this.isAdmin) {
      if (!this.composeVolunteerId) return;
      this.messageService.sendToVolunteer({
        volunteerId: this.composeVolunteerId,
        content: this.composeContent.trim()
      }).pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.composeContent = '';
            this.composeVolunteerSearch = '';
            this.composeVolunteerId = '';
            this.loadMessageSummary();
          }
        });
      return;
    }

    if (!this.adminTargetUserId) return;
    this.messageService.sendMessage({
      toUserId: this.adminTargetUserId,
      content: this.composeContent.trim()
    }).pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.composeContent = '';
          this.loadMessageSummary();
        }
      });
  }

  onVolunteerComposeSelected(event: MatAutocompleteSelectedEvent): void {
    const volunteer = event.option.value as AdminVolunteerListItem;
    if (!volunteer) {
      this.composeVolunteerId = '';
      return;
    }
    this.composeVolunteerId = volunteer.id;
    this.composeVolunteerSearch = this.getVolunteerDisplay(volunteer);
  }

  onComposeContentChange(value: string): void {
    this.composeContent = value;
  }

  onComposeVolunteerSearchChange(value: string): void {
    this.composeVolunteerSearch = value;
    this.composeVolunteerId = '';
  }

  get filteredComposeVolunteers(): AdminVolunteerListItem[] {
    const query = this.composeVolunteerSearch.trim().toLowerCase();
    if (!query) return this.volunteers;
    return this.volunteers.filter(volunteer => {
      const fullName = `${volunteer.firstName} ${volunteer.lastName}`.toLowerCase();
      return fullName.includes(query) || volunteer.email.toLowerCase().includes(query);
    });
  }

  getVolunteerDisplay(volunteer: AdminVolunteerListItem): string {
    return `${volunteer.firstName} ${volunteer.lastName} (${volunteer.email})`;
  }

  private loadMessageSummary(): void {
    this.messageService.getUnreadCount()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: count => {
          this.unreadCount = count;
        }
      });

    this.messageService.getInbox()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: messages => {
          this.recentMessages = [...messages]
            .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
            .slice(0, 5);
          const adminMessage = messages.find(msg => msg.fromUserName.toLowerCase().includes('admin'));
          this.adminTargetUserId = adminMessage?.fromUserId ?? this.adminTargetUserId;
        }
      });
  }

  private loadVolunteers(): void {
    if (this.volunteers.length > 0) return;
    this.adminDashboardService.getVolunteers()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: data => {
          this.volunteers = data;
        }
      });
  }
}
