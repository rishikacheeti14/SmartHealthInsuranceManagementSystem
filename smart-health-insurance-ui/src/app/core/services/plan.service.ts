import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { ApiService } from './api.service';

@Injectable({
    providedIn: 'root'
})
export class PlanService {
    constructor(private api: ApiService) { }

    getAllPlans(): Observable<any[]> {
        return this.api.get<any[]>('InsurancePlans');
    }

    getActivePlans(): Observable<any[]> {
        return this.api.get<any[]>('InsurancePlans/active');
    }

    getPlanById(id: number): Observable<any> {
        return this.api.get<any>(`InsurancePlans/${id}`);
    }

    createPlan(plan: any): Observable<any> {
        return this.api.post('InsurancePlans', plan);
    }

    updatePlan(id: number, plan: any): Observable<any> {
        return this.api.put(`InsurancePlans/${id}`, plan);
    }

    deletePlan(id: number): Observable<any> {
        return this.api.delete(`InsurancePlans/${id}`);
    }

    getPagedPlans(page: number, pageSize: number, sortColumn: string, isAscending: boolean, searchTerm?: string): Observable<any> {
        let params = new HttpParams()
            .set('page', page.toString())
            .set('pageSize', pageSize.toString())
            .set('sortColumn', sortColumn)
            .set('isAscending', isAscending.toString());

        if (searchTerm) {
            params = params.set('searchTerm', searchTerm);
        }

        return this.api.get<any>('InsurancePlans/paged', params);
    }
}
