import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MessageItem, CreateMessageDto, SendToVolunteerDto } from '../models/message.model';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  private apiUrl = `${environment.apiUrl}/messages`;

  constructor(private http: HttpClient) {}

  getInbox(): Observable<MessageItem[]> {
    return this.http.get<MessageItem[]>(`${this.apiUrl}/inbox`);
  }

  getSent(): Observable<MessageItem[]> {
    return this.http.get<MessageItem[]>(`${this.apiUrl}/sent`);
  }

  getUnreadCount(): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/unread-count`);
  }

  getConversation(otherUserId: string): Observable<MessageItem[]> {
    return this.http.get<MessageItem[]>(`${this.apiUrl}/conversation/${otherUserId}`);
  }

  sendMessage(dto: CreateMessageDto): Observable<MessageItem> {
    return this.http.post<MessageItem>(`${this.apiUrl}/send`, dto);
  }

  sendToVolunteer(dto: SendToVolunteerDto): Observable<MessageItem> {
    return this.http.post<MessageItem>(`${this.apiUrl}/send-to-volunteer`, dto);
  }

  markAsRead(messageId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/read`, {});
  }
}
