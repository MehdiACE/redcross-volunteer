import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface SendCommunicationRequest {
  segment: string;
  channels: number; // 1 = Email, 2 = SMS, 3 = Both
  language: string;
  subject: string;
  bodyTemplate: string;
  recipientVolunteerIds: string[] | null;
}

export interface CommunicationMessageDto {
  id: string;
  segment: string;
  channels: number;
  language: string;
  subject: string;
  bodyTemplate: string;
  sentAt: string;
  createdBy: string;
  totalRecipients: number;
  queuedCount: number;
  sentCount: number;
  failedCount: number;
  bouncedCount: number;
}

export interface CommunicationRecipientDto {
  id: string;
  messageId: string;
  recipientType: number;
  volunteerId: string | null;
  volunteerName: string | null;
  recipientEmail: string | null;
  recipientPhone: string | null;
  channel: number;
  deliveryStatus: number;
  deliveredAt: string | null;
  retriedCount: number;
  lastError: string | null;
  messageSubject: string;
  messageSentAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class CommunicationService {
  constructor(private api: ApiService) {}

  sendCommunication(request: SendCommunicationRequest): Observable<CommunicationMessageDto> {
    return this.api.post<CommunicationMessageDto>('/communications', request);
  }

  getRecentCommunications(count: number = 50): Observable<CommunicationMessageDto[]> {
    return this.api.get<CommunicationMessageDto[]>(`/communications?count=${count}`);
  }

  getCommunication(id: string): Observable<CommunicationMessageDto> {
    return this.api.get<CommunicationMessageDto>(`/communications/${id}`);
  }

  getVolunteerHistory(volunteerId: string): Observable<CommunicationRecipientDto[]> {
    return this.api.get<CommunicationRecipientDto[]>(`/communications/volunteer/${volunteerId}`);
  }

  processQueue(maxRecipients: number = 100): Observable<{ processed: number; succeeded: number }> {
    return this.api.post<{ processed: number; succeeded: number }>(`/communications/process-queue?maxRecipients=${maxRecipients}`, {});
  }
}
