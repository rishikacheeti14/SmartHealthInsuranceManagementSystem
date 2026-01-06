import { Component, OnInit, ChangeDetectorRef, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort, MatSort } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { PolicyService } from '../../../core/services/policy.service';
import { timeout, finalize, Subject, Subscription, debounceTime, distinctUntilChanged } from 'rxjs';
import { EditPolicyDialogComponent } from '../edit-policy-dialog/edit-policy-dialog.component';

@Component({
  selector: 'app-my-enrollments',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatDialogModule,
    MatPaginatorModule,
    MatSortModule,
    MatInputModule,
    MatFormFieldModule
  ],
  template: `
    <div class="enrollments-container">
      <div style="display: flex; justify-content: space-between; align-items: center;">
        <h2>My Enrollments</h2>
        <mat-form-field appearance="outline" style="width: 300px; font-size: 14px;">
            <mat-label>Search</mat-label>
            <input matInput (keyup)="onSearch($event)" placeholder="Search Policy, Customer, Plan...">
            <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>
      </div>
      
      <!-- ERROR MESSAGE -->
      <div *ngIf="errorMessage" style="background: #ffebee; color: #c62828; padding: 10px; margin-bottom: 10px; border: 1px solid #ef5350;">
        <strong>Error:</strong> {{ errorMessage }}
      </div>

      <!-- LOADING INDICATOR -->
      <div *ngIf="isLoading" style="color: gray; padding: 10px;">
        Loading policies...
      </div>

      <table mat-table [dataSource]="policies" class="mat-elevation-z8" matSort (matSortChange)="onSortChange($event)" *ngIf="!isLoading && !errorMessage && policies.length > 0">
        
        <!-- ... columns ... -->


        <!-- Policy Number Column -->
        <ng-container matColumnDef="policyNumber">
          <th mat-header-cell *matHeaderCellDef mat-sort-header> Policy Number </th>
          <td mat-cell *matCellDef="let policy"> {{policy.policyNumber}} </td>
        </ng-container>

        <!-- Customer Name Column -->
        <ng-container matColumnDef="customerName">
            <th mat-header-cell *matHeaderCellDef mat-sort-header> Customer </th>
            <td mat-cell *matCellDef="let policy"> 
                {{policy.customerName}}
            </td>
        </ng-container>

        <!-- Plan Name Column -->
        <ng-container matColumnDef="planName">
          <th mat-header-cell *matHeaderCellDef mat-sort-header> Plan </th>
          <td mat-cell *matCellDef="let policy"> {{policy.planName}} </td>
        </ng-container>

        <!-- Status Column -->
        <ng-container matColumnDef="status">
          <th mat-header-cell *matHeaderCellDef mat-sort-header> Status </th>
          <td mat-cell *matCellDef="let policy"> 
            <span [class.active-status]="policy.status === 'Active'" [class.inactive-status]="policy.status !== 'Active'">
                {{ policy.status }}
            </span>
          </td>
        </ng-container>

        <!-- Date Column -->
        <ng-container matColumnDef="startDate">
          <th mat-header-cell *matHeaderCellDef mat-sort-header> Start Date </th>
          <td mat-cell *matCellDef="let policy"> {{policy.startDate | date}} </td>
        </ng-container>

        <!-- Actions Column -->
        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef> Actions </th>
          <td mat-cell *matCellDef="let policy">
            <button mat-icon-button color="accent" matTooltip="Renew Policy" (click)="onRenewPolicy(policy)">
              <mat-icon>autorenew</mat-icon>
            </button>
            <button mat-icon-button color="primary" matTooltip="Edit Policy" (click)="openEditDialog(policy)">
              <mat-icon>edit</mat-icon>
            </button>
            <button mat-icon-button *ngIf="policy.status === 'Active'" color="warn" matTooltip="Deactivate Policy" (click)="toggleStatus(policy)">
              <mat-icon>block</mat-icon>
            </button>
            <button mat-icon-button *ngIf="policy.status === 'Suspended'" color="primary" matTooltip="Activate Policy" (click)="toggleStatus(policy)">
              <mat-icon>check_circle</mat-icon>
            </button>
            <button mat-icon-button color="warn" matTooltip="Delete Policy" (click)="deletePolicy(policy)">
              <mat-icon>delete</mat-icon>
            </button>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
      </table>

      <mat-paginator *ngIf="!isLoading && !errorMessage && policies.length > 0"
        [length]="totalCount" [pageSize]="pageSize" [pageSizeOptions]="[5, 10, 25]"
        (page)="onPageChange($event)" aria-label="Select page">
      </mat-paginator>
      
      <div *ngIf="!isLoading && !errorMessage && policies.length === 0" class="no-data">
        <p>No enrollments found.</p>
      </div>
    </div>
  `,
  styles: [`
    .enrollments-container { padding: 20px; }
    table { width: 100%; margin-top: 20px; }
    .active-status { color: green; font-weight: bold; }
    .inactive-status { color: red; }
    .no-data { text-align: center; margin-top: 20px; color: gray; }
  `]
})
export class MyEnrollmentsComponent implements OnInit {
  @ViewChild(MatSort) sort!: MatSort;
  policies: any[] = [];
  errorMessage: string = '';
  isLoading: boolean = false;
  displayedColumns: string[] = ['policyNumber', 'customerName', 'planName', 'status', 'startDate', 'actions'];
  totalCount = 0;
  pageSize = 5;
  pageIndex = 0;
  sortColumn = 'PolicyId';
  isAscending = false;

  searchTerm: string = '';
  private searchSubject = new Subject<string>();
  private searchSub: Subscription | null = null;

  constructor(
    private policyService: PolicyService,
    private cdr: ChangeDetectorRef,
    private dialog: MatDialog
  ) { }

  ngOnInit() {
    this.loadPolicies();

    this.searchSub = this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(term => {
      this.searchTerm = term;
      this.pageIndex = 0; 
      this.loadPolicies();
    });
  }

  ngOnDestroy() {
    if (this.searchSub) this.searchSub.unsubscribe();
  }

  onSearch(event: any) {
    this.searchSubject.next(event.target.value);
  }

  loadPolicies() {
    this.isLoading = true;
    this.errorMessage = '';

    this.policyService.getPagedPolicies(this.pageIndex + 1, this.pageSize, this.sortColumn, this.isAscending, this.searchTerm)
      .pipe(
        timeout(5000),
        finalize(() => {
          this.isLoading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (response) => {
          this.policies = response.items;
          this.totalCount = response.totalCount;
        },
        error: (err) => {
          if (err.name === 'TimeoutError') {
            this.errorMessage = "The connection timed out. Please try again later.";
          } else if (err.status === 401) {
            this.errorMessage = "Your session has expired. Please log in again.";
          } else {
            this.errorMessage = "Failed to load enrollment data. Please contact support if this persists.";
          }
        }
      });
  }

  onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadPolicies();
  }

  onSortChange(event: Sort) {
    if (event.direction === 'desc') {

      this.sort.active = '';
      this.sort.direction = '';
      this.cdr.detectChanges(); 

      this.sortColumn = 'PolicyId';
      this.isAscending = false;
      this.pageIndex = 0;
      this.loadPolicies();
      return;
    }

    if (!event.active || event.direction === '') {
      this.sortColumn = 'PolicyId';
      this.isAscending = false;
    } else {
      this.sortColumn = event.active;
      if (this.sortColumn === 'policyNumber') this.sortColumn = 'PolicyNumber';
      if (this.sortColumn === 'customerName') this.sortColumn = 'UserId';
      if (this.sortColumn === 'planName') this.sortColumn = 'PlanId';
      if (this.sortColumn === 'status') this.sortColumn = 'Status';
      if (this.sortColumn === 'startDate') this.sortColumn = 'StartDate';

      this.isAscending = event.direction === 'asc';
    }

    this.pageIndex = 0;
    this.loadPolicies();
  }

  toggleStatus(policy: any) {
    const action = policy.status === 'Active' ? 'Deactivate' : 'Activate';
    if (confirm(`Are you sure you want to ${action} policy ${policy.policyNumber}?`)) {
      this.isLoading = true;
      this.policyService.togglePolicyStatus(policy.policyId).subscribe({
        next: (res: any) => {
          alert(res.message);
          this.loadPolicies();
        },
        error: (err) => {
          alert(`Failed to ${action}: ` + (err.error?.message || err.message));
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
    }
  }

  onRenewPolicy(policy: any) {
    if (confirm(`Renew Policy ${policy.policyNumber}?\n\nThis will increase the premium by 8% and extend the duration.\nCurrent Status: ${policy.status}`)) {
      this.isLoading = true;
      this.policyService.renewPolicy(policy.policyId).subscribe({
        next: (res: any) => {
          alert(res.message);
          this.loadPolicies();
        },
        error: (err) => {
          alert('Renewal Failed: ' + (err.error?.message || err.message));
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
    }
  }

  openEditDialog(policy: any) {
    const dialogRef = this.dialog.open(EditPolicyDialogComponent, {
      width: '400px',
      data: {
        planId: policy.planId,
        startDate: policy.startDate
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.isLoading = true;
        this.policyService.updatePolicy(policy.policyId, {
          ...result,
          firstName: policy.user?.firstName || '',
          lastName: policy.user?.lastName || '',
          email: policy.user?.email || '',
          dateOfBirth: policy.user?.dateOfBirth || new Date()
        }).subscribe({
          next: () => {
            this.loadPolicies();
          },
          error: (err) => {
            alert('Update Failed: ' + (err.error?.message || err.message));
            this.isLoading = false;
            this.cdr.detectChanges();
          }
        });
      }
    });
  }

  deletePolicy(policy: any) {
    if (confirm(`Are you sure you want to delete policy ${policy.policyNumber}?`)) {
      this.isLoading = true;
      this.policyService.deletePolicy(policy.policyId).subscribe({
        next: () => {
          this.loadPolicies();
        },
        error: (err) => {
          alert('Delete Failed: ' + (err.error?.message || err.message));
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
    }
  }
}
