import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { ApiService } from './api.service';

@Injectable({
    providedIn: 'root'
})
export class HospitalService {
    constructor(private api: ApiService) { }

    getHospitals(): Observable<any[]> {
        return this.api.get<any[]>('Hospitals');
    }

    getHospitalById(id: number): Observable<any> {
        return this.api.get<any>(`Hospitals/${id}`);
    }

    createHospital(hospital: any): Observable<any> {
        return this.api.post('Hospitals', hospital);
    }

    updateHospital(id: number, hospital: any): Observable<any> {
        return this.api.put(`Hospitals/${id}`, hospital);
    }

    deleteHospital(id: number): Observable<any> {
        return this.api.delete(`Hospitals/${id}`);
    }

    getPagedHospitals(page: number, pageSize: number, sortColumn: string, isAscending: boolean, searchTerm?: string): Observable<any> {
        let params = new HttpParams()
            .set('page', page.toString())
            .set('pageSize', pageSize.toString())
            .set('sortColumn', sortColumn)
            .set('isAscending', isAscending.toString());

        if (searchTerm) {
            params = params.set('searchTerm', searchTerm);
        }

        return this.api.get<any>('Hospitals/paged', params);
    }
}
