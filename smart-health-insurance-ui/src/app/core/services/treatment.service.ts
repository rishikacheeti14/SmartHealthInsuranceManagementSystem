import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { ApiService } from './api.service';

@Injectable({
    providedIn: 'root'
})
export class TreatmentService {
    constructor(private api: ApiService) { }

    submitTreatment(treatment: any): Observable<any> {
        return this.api.post('Treatments', treatment);
    }

    getMyTreatments(): Observable<any[]> {
        return this.api.get<any[]>('Treatments/my'); 
    }

    getHospitalTreatments(): Observable<any[]> {
        return this.api.get<any[]>('Treatments/hospital-v2');
    }

    updateTreatment(id: number, treatment: any): Observable<any> {
        return this.api.put(`Treatments/${id}`, treatment);
    }

    deleteTreatment(id: number): Observable<any> {
        return this.api.delete(`Treatments/${id}`);
    }

    getPagedTreatments(page: number, pageSize: number, sortColumn: string, isAscending: boolean, searchTerm?: string): Observable<any> {
        let params = new HttpParams()
            .set('page', page.toString())
            .set('pageSize', pageSize.toString())
            .set('sortColumn', sortColumn)
            .set('isAscending', isAscending.toString());

        if (searchTerm) {
            params = params.set('searchTerm', searchTerm);
        }

        return this.api.get<any>('Treatments/paged', params);
    }
}
