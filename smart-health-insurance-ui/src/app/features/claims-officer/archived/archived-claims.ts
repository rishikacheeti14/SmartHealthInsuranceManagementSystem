import { Component, OnInit, ChangeDetectorRef, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort, MatSort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { ClaimService } from '../../../core/services/claim.service';

@Component({
    selector: 'app-archived-claims',
    standalone: true,
    imports: [
        CommonModule,
        MatTableModule,
        MatCardModule,
        MatChipsModule,
        MatPaginatorModule,
        MatSortModule,
        MatFormFieldModule,
        MatInputModule,
        MatIconModule
    ],
    templateUrl: './archived-claims.html',
    styleUrls: ['./archived-claims.css']
})
export class ArchivedClaims implements OnInit {
    @ViewChild(MatSort) sort!: MatSort;
    archivedClaims: any[] = [];
    displayedColumns: string[] = ['claimNumber', 'policyNumber', 'hospital', 'amount', 'status', 'date', 'actions'];
    loading = true;

    totalCount = 0;
    pageSize = 5;
    pageIndex = 0;
    sortColumn = 'SubmittedAt';
    isAscending = false;

    searchTerm = '';
    private searchTermSubject = new Subject<string>();
    private destroy$ = new Subject<void>();

    constructor(
        private claimService: ClaimService,
        private cdr: ChangeDetectorRef
    ) { }

    ngOnInit() {
        this.loadArchivedClaims();

        this.searchTermSubject.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            takeUntil(this.destroy$)
        ).subscribe(term => {
            this.searchTerm = term;
            this.pageIndex = 0;
            this.loadArchivedClaims();
        });
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }

    onSearch(term: string) {
        this.searchTermSubject.next(term);
    }

    loadArchivedClaims() {
        this.loading = true;
        this.claimService.getPagedClaims(this.pageIndex + 1, this.pageSize, this.sortColumn, this.isAscending, this.searchTerm, true).subscribe({
            next: (res) => {
                this.archivedClaims = res.items || [];
                this.totalCount = res.totalCount;
                this.loading = false;
                this.cdr.detectChanges();
            },
            error: (err) => {
                console.error('Error fetching archives', err);
                this.loading = false;
                this.cdr.detectChanges();
            }
        });
    }

    onPageChange(event: PageEvent) {
        this.pageIndex = event.pageIndex;
        this.pageSize = event.pageSize;
        this.loadArchivedClaims();
    }

    onSortChange(event: Sort) {
        if (event.direction === 'desc') {
            this.sort.active = '';
            this.sort.direction = '';
            this.cdr.detectChanges();
            this.sortColumn = 'SubmittedAt'; 
            this.isAscending = false;
            this.pageIndex = 0;
            this.loadArchivedClaims();
            return;
        }

        this.sortColumn = event.active;
        if (this.sortColumn === 'claimNumber') this.sortColumn = 'ClaimNumber';
        if (this.sortColumn === 'amount') this.sortColumn = 'ClaimAmount';
        if (this.sortColumn === 'policyNumber') this.sortColumn = 'PolicyId';
        if (this.sortColumn === 'status') this.sortColumn = 'Status';
        if (this.sortColumn === 'date') this.sortColumn = 'ReviewedAt';

        this.isAscending = event.direction === 'asc';
        this.pageIndex = 0;
        this.loadArchivedClaims();
    }

    getStatusLabel(status: any): string {
        const s = status?.toString().toLowerCase();
        if (s === 'paid' || s === '5') return 'Paid';
        if (s === 'rejected' || s === '4') return 'Rejected';
        if (s === 'approved' || s === '3') return 'Approved';
        return s ? s.charAt(0).toUpperCase() + s.slice(1) : 'Processed';
    }

    getStatusClass(status: any): string {
        const s = status?.toString().toLowerCase();
        if (s === 'paid' || s === '5' || s === 'approved' || s === '3') return 'status-paid';
        return 'status-rejected';
    }

    editClaim(claim: any) {
        alert(`Edit functionality is restricted for archived claim #${claim.claimNumber}. Only Admins can modify historical records.`);
    }

    deleteClaim(claim: any) {
        if (confirm(`Are you sure you want to delete archived claim ${claim.claimNumber}? This action cannot be undone.`)) {
            this.claimService.deleteClaim(claim.claimId).subscribe({
                next: () => {
                    alert('Claim deleted successfully');
                    this.loadArchivedClaims();
                },
                error: (err) => alert('Error deleting claim: ' + (err.error?.message || err.message))
            });
        }
    }
}
