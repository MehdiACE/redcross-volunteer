export interface MessageItem {
  id: string;
  fromUserId: string;
  fromUserName: string;
  toUserName?: string;
  content: string;
  isRead: boolean;
  createdAt: Date;
  readAt?: Date;
}

export interface CreateMessageDto {
  toUserId?: string;
  toVolunteerId?: string;
  content: string;
}

export interface SendToVolunteerDto {
  volunteerId: string;
  content: string;
}
