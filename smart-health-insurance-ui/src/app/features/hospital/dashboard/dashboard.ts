import { Component, OnInit, ChangeDetectorRef, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort, MatSort } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { Router } from '@angular/router';
import { TreatmentService } from '../../../core/services/treatment.service';
import { ClaimService } from '../../../core/services/claim.service';
import { TreatmentEditDialog } from './treatment-edit-dialog';
import { ClaimUpdateDialog } from './claim-update-dialog';
import { TreatmentDetailsDialog } from './treatment-details-dialog';
import { Subject, debounceTime, distinctUntilChanged, Subscription } from 'rxjs';

@Component({
  selector: 'app-hospital-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatTableModule,
    MatIconModule,
    MatDialogModule,
    MatPaginatorModule,
    MatSortModule,
    MatInputModule,
    MatFormFieldModule
  ],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class Dashboard implements OnInit, OnDestroy {
  @ViewChild(MatSort) sort!: MatSort;
  treatments: any[] = [];
  initiatedClaims: any[] = [];
  displayedColumns: string[] = ['date', 'patient', 'diagnosis', 'cost', 'actions'];
  claimColumns: string[] = ['id', 'patient', 'description', 'actions'];

  searchTerm: string = '';
  private searchSubject = new Subject<string>();
  private searchSub: Subscription | null = null;

  constructor(
    private router: Router,
    private treatmentService: TreatmentService,
    private claimService: ClaimService,
    private cdr: ChangeDetectorRef,
    private dialog: MatDialog
  ) { }

  totalCount = 0;
  pageSize = 5;
  pageIndex = 0;
  sortColumn = 'TreatmentId';
  isAscending = false;

  ngOnInit() {
    this.loadHistory();
    this.loadInitiatedClaims();

    this.searchSub = this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(term => {
      this.searchTerm = term;
      this.pageIndex = 0; 
      this.loadHistory();
    });
  }

  ngOnDestroy() {
    if (this.searchSub) this.searchSub.unsubscribe();
  }

  onSearch(event: any) {
    this.searchSubject.next(event.target.value);
  }

  loadHistory() {
    this.treatmentService.getPagedTreatments(this.pageIndex + 1, this.pageSize, this.sortColumn, this.isAscending, this.searchTerm).subscribe({
      next: (response) => {
        this.treatments = response.items;
        this.totalCount = response.totalCount;
        this.cdr.detectChanges();
      },
      error: () => { }
    });
  }

  viewDetails(treatment: any) {
    this.dialog.open(TreatmentDetailsDialog, {
      width: '500px',
      data: treatment
    });
  }

  loadInitiatedClaims() {

    this.claimService.getPagedClaims(1, 100, 'ClaimId', false, 'Initiated').subscribe({
      next: (response) => {
        this.initiatedClaims = response.items;
        this.cdr.detectChanges();
      },
      error: () => { }
    });
  }

  provideTreatmentDetails(claim: any) {
    const dialogRef = this.dialog.open(ClaimUpdateDialog, {
      width: '500px',
      data: claim
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.claimService.updateClaimByHospital(claim.claimId, result).subscribe({
          next: () => {
            alert('Details submitted successfully!');
            this.loadInitiatedClaims();
            this.loadHistory();
          },
          error: (err) => alert('Update failed: ' + (err.error?.message || err.message))
        });
      }
    });
  }

  onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadHistory();
  }

  onSortChange(event: Sort) {
    if (event.direction === 'desc') {
      this.sort.active = '';
      this.sort.direction = '';
      this.cdr.detectChanges();
      this.sortColumn = 'TreatmentId'; 
      this.isAscending = false;
      this.pageIndex = 0;
      this.loadHistory();
      return;
    }

    this.sortColumn = event.active;
    if (this.sortColumn === 'date') this.sortColumn = 'TreatmentDate';
    if (this.sortColumn === 'patient') this.sortColumn = 'CustomerId';
    if (this.sortColumn === 'diagnosis') this.sortColumn = 'Diagnosis';
    if (this.sortColumn === 'cost') this.sortColumn = 'TreatmentCost';

    this.isAscending = event.direction === 'asc';
    this.pageIndex = 0;
    this.loadHistory();
  }

  navigateTo(path: string) {
    this.router.navigate(['/hospital', path]);
  }

  deleteTreatment(id: number) {
    if (confirm('Are you sure you want to delete this treatment record?')) {
      const token = localStorage.getItem('token');
      fetch(`https://localhost:7049/api/Treatments/${id}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      })
        .then(res => {
          if (res.ok) {
            this.loadHistory();
          } else {
            alert('Delete failed');
          }
        })
        .catch(() => { });
    }
  }

  editTreatment(treatment: any) {
    const dialogRef = this.dialog.open(TreatmentEditDialog, {
      width: '500px',
      data: treatment
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        const token = localStorage.getItem('token');
        fetch(`https://localhost:7049/api/Treatments/${treatment.treatmentId}`, {
          method: 'PUT',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({
            ...result,
            policyNumber: treatment.policy?.policyNumber || ''
          })
        })
          .then(res => {
            if (res.ok) {
              this.loadHistory();
            } else {
              alert('Update failed');
            }
          })
          .catch(() => { });
      }
    });
  }
}


