import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { ApiService } from './api.service';

@Injectable({
    providedIn: 'root'
})
export class PolicyService {
    constructor(private api: ApiService) { }

    getAllPlans(): Observable<any[]> {
        return this.api.get<any[]>('InsurancePlans/active');
    }

    enrollPolicy(enrollmentData: any): Observable<any> {
        return this.api.post('Policies/enroll', enrollmentData);
    }

   
    getMyPolicies(): Observable<any[]> {
        return this.api.get<any[]>('Policies/my-policies');
    }


    getAgentPolicies(): Observable<any[]> {
        return this.api.get<any[]>('Policies/agent-policies');
    }

    getAllPolicies(): Observable<any[]> {
        return this.api.get<any[]>('Policies');
    }

    updatePolicy(id: number, data: any): Observable<any> {
        return this.api.put(`Policies/${id}`, data);
    }

    deletePolicy(id: number): Observable<any> {
        return this.api.delete(`Policies/${id}`);
    }

    getPolicyByNumber(policyNumber: string): Observable<any> {
        return this.api.get<any>(`Policies/lookup/${policyNumber}`);
    }

    renewPolicy(id: number): Observable<any> {
        return this.api.post(`Policies/${id}/renew`, {});
    }

    togglePolicyStatus(id: number): Observable<any> {
        return this.api.post(`Policies/${id}/toggle-status`, {});
    }

    getPagedPolicies(page: number, pageSize: number, sortColumn: string, isAscending: boolean, searchTerm?: string): Observable<any> {
        let params = new HttpParams()
            .set('page', page.toString())
            .set('pageSize', pageSize.toString())
            .set('sortColumn', sortColumn)
            .set('isAscending', isAscending.toString());

        if (searchTerm) {
            params = params.set('searchTerm', searchTerm);
        }

        return this.api.get<any>('Policies/paged', params);
    }
}
