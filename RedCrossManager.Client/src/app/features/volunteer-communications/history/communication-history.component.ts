import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import { CommunicationService, CommunicationMessageDto } from '../../../core/services/communication.service';

@Component({
  selector: 'app-communication-history',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    TranslateModule
  ],
  templateUrl: './communication-history.component.html',
  styleUrls: ['./communication-history.component.scss']
})
export class CommunicationHistoryComponent implements OnInit {
  messages: CommunicationMessageDto[] = [];
  displayedColumns: string[] = ['sentAt', 'segment', 'subject', 'channels', 'stats', 'actions'];
  isLoading = false;

  constructor(private communicationService: CommunicationService) {}

  ngOnInit(): void {
    this.loadHistory();
  }

  loadHistory(): void {
    this.isLoading = true;
    this.communicationService.getRecentCommunications(50).subscribe({
      next: (messages) => {
        this.messages = messages;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Failed to load communication history:', error);
        this.isLoading = false;
      }
    });
  }

  getChannelLabel(channels: number): string {
    const labels: string[] = [];
    if (channels & 1) labels.push('Email');
    if (channels & 2) labels.push('SMS');
    return labels.join(' + ') || 'None';
  }

  getTotalRecipients(message: CommunicationMessageDto): number {
    return message.queuedCount + message.sentCount + message.failedCount + message.bouncedCount;
  }

  getSuccessRate(message: CommunicationMessageDto): number {
    const total = this.getTotalRecipients(message);
    return total > 0 ? Math.round((message.sentCount / total) * 100) : 0;
  }

  viewDetails(messageId: string): void {
    // Navigate to detailed view or open dialog
    console.log('View details for message:', messageId);
  }
}
