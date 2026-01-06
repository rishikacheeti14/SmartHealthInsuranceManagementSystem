import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PolicyDistributionDto {
    policyType: string;
    status: string;
    count: number;
}

export interface ClaimAnalyticsDto {
    status: string;
    hospitalName: string;
    totalAmount: number;
    totalApprovedAmount: number;
    count: number;
}

export interface PlanFinancialDto {
    planName: string;
    totalPremium: number;
    totalPayout: number;
}

export interface PremiumVsPayoutDto {
    totalPremiumsCollected: number;
    totalClaimsPaid: number;
    totalCoverageAmount: number;
    remainingCoverage: number;
    planBreakdown: PlanFinancialDto[];
}

export interface HighValueClaimDto {
    claimId: number;
    claimNumber: string;
    claimAmount: number;
    hospitalName: string;
    diagnosis: string;
    submittedAt: string;
}

@Injectable({
    providedIn: 'root'
})
export class ReportService {
    private apiUrl = `${environment.apiUrl}/reports`;

    constructor(private http: HttpClient) { }

    getPolicyDistribution(): Observable<PolicyDistributionDto[]> {
        return this.http.get<PolicyDistributionDto[]>(`${this.apiUrl}/policies-distribution`);
    }

    getClaimsAnalytics(): Observable<ClaimAnalyticsDto[]> {
        return this.http.get<ClaimAnalyticsDto[]>(`${this.apiUrl}/claims-analytics`);
    }

    getPremiumVsPayout(): Observable<PremiumVsPayoutDto> {
        return this.http.get<PremiumVsPayoutDto>(`${this.apiUrl}/premium-vs-payout`);
    }

    getHighValueClaims(threshold: number = 100000): Observable<HighValueClaimDto[]> {
        return this.http.get<HighValueClaimDto[]>(`${this.apiUrl}/high-value-claims?threshold=${threshold}`);
    }

    getDashboardStats(): Observable<any> {
        return this.http.get<any>(`${this.apiUrl}/dashboard-stats`);
    }

    getRevenueTrend(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/revenue-trend`);
    }
}
