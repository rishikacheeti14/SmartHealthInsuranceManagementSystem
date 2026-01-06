import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface Notification {
    notificationId: number;
    userId: number;
    message: string;
    isRead: boolean;
    createdAt: string;
}

@Injectable({
    providedIn: 'root'
})
export class NotificationService {
    constructor(private api: ApiService) { }

    getMyNotifications(): Observable<Notification[]> {
        return this.api.get<Notification[]>('Notifications/my');
    }

    markAsRead(id: number): Observable<any> {
        return this.api.put(`Notifications/${id}/read`, {});
    }

    markAllAsRead(): Observable<any> {
        return this.api.put('Notifications/read-all', {});
    }
}
