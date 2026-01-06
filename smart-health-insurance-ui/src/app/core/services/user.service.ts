import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { ApiService } from './api.service';

@Injectable({
    providedIn: 'root'
})
export class UserService {
    constructor(private api: ApiService) { }

    getAllUsers(): Observable<any[]> {
        return this.api.get<any[]>('Users');
    }

    createStaff(user: any): Observable<any> {
        return this.api.post('Users', user);
    }

    updateUser(id: number, user: any): Observable<any> {
        return this.api.put(`Users/${id}`, user);
    }

    toggleStatus(id: number, isActive: boolean): Observable<any> {
        const action = isActive ? 'deactivate' : 'activate';
        return this.api.put(`Users/${id}/${action}`, {});
    }

    deleteUser(id: number): Observable<any> {
        return this.api.delete(`Users/${id}`);
    }

    getPagedUsers(page: number, pageSize: number, sortColumn: string, isAscending: boolean, searchTerm?: string): Observable<any> {
        let params = new HttpParams()
            .set('page', page.toString())
            .set('pageSize', pageSize.toString())
            .set('sortColumn', sortColumn)
            .set('isAscending', isAscending.toString());

        if (searchTerm) {
            params = params.set('searchTerm', searchTerm);
        }

        return this.api.get<any>('Users/paged', params);
    }
}
