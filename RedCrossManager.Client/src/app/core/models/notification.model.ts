export interface NotificationItem {
  id: string;
  title: string;
  message: string;
  type: 'Info' | 'Success' | 'Warning' | 'Error' | string;
  isRead: boolean;
  createdAt: string;
  actionUrl?: string | null;
}
