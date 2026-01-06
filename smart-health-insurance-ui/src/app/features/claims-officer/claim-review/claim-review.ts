import { Component, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort, MatSort } from '@angular/material/sort';
import { ClaimService } from '../../../core/services/claim.service';

@Component({
  selector: 'app-claim-review',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatPaginatorModule,
    MatSortModule
  ],
  templateUrl: './claim-review.html',
  styleUrls: ['./claim-review.css']
})
export class ClaimReview implements OnInit {
  @ViewChild(MatSort) sort!: MatSort;
  claims: any[] = [];
  displayedColumns: string[] = ['id', 'customer', 'policy', 'amount', 'actions'];
  expandedElement: any = null;

  totalCount = 0;
  pageSize = 5;
  pageIndex = 0;
  sortColumn = 'ClaimId';
  isAscending = false;

  constructor(
    private claimService: ClaimService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.loadClaims();
  }

  loadClaims() {
    this.claimService.getPagedClaims(this.pageIndex + 1, this.pageSize, this.sortColumn, this.isAscending, 'Submitted').subscribe({
      next: (response) => {
        this.claims = (response.items || []).map((c: any) => ({
          ...c,
          approvedAmountRef: c.claimAmount,
          rejectionReasonRef: ''
        }));
        this.totalCount = response.totalCount;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error fetching claims', err)
    });
  }

  onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadClaims();
  }

  onSortChange(event: Sort) {
    if (event.direction === 'desc') {
      this.sort.active = '';
      this.sort.direction = '';
      this.cdr.detectChanges();
      this.sortColumn = 'ClaimId';
      this.isAscending = false;
      this.pageIndex = 0;
      this.loadClaims();
      return;
    }

    this.sortColumn = event.active;
    if (this.sortColumn === 'id') this.sortColumn = 'ClaimId';
    if (this.sortColumn === 'policy') this.sortColumn = 'PolicyId';
    if (this.sortColumn === 'amount') this.sortColumn = 'ClaimAmount';

    this.isAscending = event.direction === 'asc';
    this.pageIndex = 0;
    this.loadClaims();
  }

  expand(claim: any) {
    this.expandedElement = claim;
  }

  submitReview(claim: any, isApproved: boolean) {
    const reviewDto = {
      isApproved: isApproved,
      approvedAmount: isApproved ? claim.approvedAmountRef : 0,
      rejectionReason: isApproved ? '' : (claim.rejectionReasonRef || 'Rejected')
    };

    this.claimService.reviewClaim(claim.claimId, reviewDto).subscribe({
      next: () => {
        alert(`Claim #${claim.claimId} ${isApproved ? 'Approved' : 'Rejected'}`);
        this.loadClaims();
        this.expandedElement = null;
      },
      error: (err) => {
        alert('Review failed: ' + (err.error?.message || err.message));
      }
    });
  }
}
