import { Component, OnInit, ChangeDetectorRef, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort, MatSort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { ClaimService } from '../../../core/services/claim.service';
import { PolicySelectionDialog } from './policy-selection-dialog';

import { ClaimDetailsDialog } from './claim-details-dialog';

import { EditClaimDialog } from './edit-claim-dialog';

@Component({
  selector: 'app-my-claims',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './my-claims.html',
  styleUrls: ['./my-claims.css']
})
export class MyClaims implements OnInit {
  @ViewChild(MatSort) sort!: MatSort;
  claims: any[] = [];
  displayedColumns: string[] = ['id', 'date', 'amount', 'status', 'actions'];

  totalCount = 0;
  pageSize = 5;
  pageIndex = 0;
  sortColumn = 'ClaimId';
  isAscending = false;

  searchTerm = '';
  private searchTermSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  constructor(
    private claimService: ClaimService,
    private cdr: ChangeDetectorRef,
    private dialog: MatDialog
  ) { }

  ngOnInit() {
    this.loadClaims();

    this.searchTermSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(term => {
      this.searchTerm = term;
      this.pageIndex = 0;
      this.loadClaims();
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearch(term: string) {
    this.searchTermSubject.next(term);
  }

  loadClaims() {
    this.claimService.getPagedClaims(this.pageIndex + 1, this.pageSize, this.sortColumn, this.isAscending, this.searchTerm).subscribe({
      next: (response) => {
        this.claims = response.items;
        this.totalCount = response.totalCount;
        this.cdr.detectChanges();
      },
      error: () => { }
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
    if (this.sortColumn === 'date') this.sortColumn = 'SubmittedAt';
    if (this.sortColumn === 'amount') this.sortColumn = 'ClaimAmount';
    if (this.sortColumn === 'status') this.sortColumn = 'Status';

    this.isAscending = event.direction === 'asc';
    this.pageIndex = 0;
    this.loadClaims();
  }

  getStatus(status: any): string {
    const map: any = { 1: 'submitted', 2: 'review', 3: 'approved', 4: 'rejected', 5: 'paid', 6: 'initiated', 7: 'awaiting-hospital', 8: 'awaiting-policy' };
    return map[status] || (typeof status === 'string' ? status.toLowerCase() : 'unknown');
  }

  getStatusLabel(status: any): string {
    if (typeof status === 'string') return status;

    const map: any = {
      1: 'Submitted', 2: 'In Review', 3: 'Approved', 4: 'Rejected', 5: 'Paid',
      6: 'Initiated (Wait for Hospital)', 7: 'Under Hospital Review', 8: 'Provide Policy'
    };
    return map[status] || `Unknown (${status})`;
  }

  finalizeClaim(claim: any) {
    if (claim.policyId) {

      this.claimService.finalizeClaim(claim.claimId, { policyId: claim.policyId }).subscribe({
        next: () => {
          alert('Claim submitted for review!');
          this.loadClaims();
        },
        error: (err) => alert('Finalization failed: ' + (err.error?.message || err.message))
      });
    } else {
      const dialogRef = this.dialog.open(PolicySelectionDialog, {
        width: '500px',
        data: claim
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result) {
          this.claimService.finalizeClaim(claim.claimId, result).subscribe({
            next: () => {
              alert('Claim finalized and submitted successfully!');
              this.loadClaims();
            },
            error: (err) => alert('Finalization failed: ' + (err.error?.message || err.message))
          });
        }
      });
    }
  }

  viewDetails(claim: any) {
    this.dialog.open(ClaimDetailsDialog, {
      width: '600px',
      data: claim
    });
  }

  editClaim(claim: any) {
    const dialogRef = this.dialog.open(EditClaimDialog, {
      width: '500px',
      data: { description: claim.treatmentDescription }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.claimService.updateClaim(claim.claimId, result).subscribe({
          next: () => {
            alert('Claim updated successfully');
            this.loadClaims();
          },
          error: (err) => alert('Update failed: ' + (err.error?.message || err.message))
        });
      }
    });
  }

  deleteClaim(claim: any) {
    if (confirm('Are you sure you want to delete this claim?')) {
      this.claimService.deleteClaim(claim.claimId).subscribe({
        next: () => {
          alert('Claim deleted successfully');
          this.loadClaims();
        },
        error: (err) => alert('Delete failed: ' + (err.error?.message || err.message))
      });
    }
  }
}
