export interface AppNotification {
  id: number;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
  link: string | null;
}