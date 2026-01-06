import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { HttpParams } from '@angular/common/http';

@Injectable({
    providedIn: 'root'
})
export class ClaimService {
    constructor(private api: ApiService) { }

    submitClaim(claim: any): Observable<any> {
        return this.api.post('Claims', claim);
    }

    initiateClaim(dto: any): Observable<any> {
        return this.api.post('Claims/initiate', dto);
    }

    updateClaimByHospital(id: number, dto: any): Observable<any> {
        return this.api.put(`Claims/${id}/hospital-update`, dto);
    }

    finalizeClaim(id: number, dto: any): Observable<any> {
        return this.api.put(`Claims/${id}/finalize`, dto);
    }

    getMyClaims(): Observable<any[]> {
        return this.api.get<any[]>('Claims/my-claims');
    }

    // Admin/Officer
    getClaimsByStatus(status: string): Observable<any[]> {
        return this.api.get<any[]>('Claims', new HttpParams().set('status', status));
    }

    reviewClaim(id: number, review: any): Observable<any> {
        return this.api.put(`Claims/${id}/review`, review);
    }

    getPagedClaims(page: number, pageSize: number, sortColumn: string, isAscending: boolean, status: string = '', isArchived: boolean = false, searchTerm?: string): Observable<any> {
        let params = new HttpParams()
            .set('page', page.toString())
            .set('pageSize', pageSize.toString())
            .set('sortColumn', sortColumn)
            .set('isAscending', isAscending.toString())
            .set('isArchived', isArchived.toString());

        if (status) {
            params = params.set('status', status);
        }

        if (searchTerm) {
            params = params.set('searchTerm', searchTerm);
        }

        return this.api.get<any>('Claims/paged', params);
    }

    deleteClaim(id: number): Observable<any> {
        return this.api.delete(`Claims/${id}`);
    }

    updateClaim(id: number, description: string): Observable<any> {
        return this.api.put(`Claims/${id}`, { treatmentDescription: description });
    }
}
