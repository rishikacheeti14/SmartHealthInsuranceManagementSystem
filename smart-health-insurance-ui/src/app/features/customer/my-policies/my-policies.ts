import { Component, OnInit, ChangeDetectorRef, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort, MatSort } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { PolicyService } from '../../../core/services/policy.service';
import { PaymentService } from '../../../core/services/payment.service';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Subject, Subscription, debounceTime, distinctUntilChanged } from 'rxjs';
import { PaymentDialog } from '../payment-dialog/payment-dialog';

@Component({
  selector: 'app-my-policies',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatInputModule,
    MatFormFieldModule,
    MatIconModule,
    MatDialogModule
  ],
  templateUrl: './my-policies.html',
  styleUrls: ['./my-policies.css']
})
export class MyPolicies implements OnInit, OnDestroy {
  @ViewChild(MatSort) sort!: MatSort;
  policies: any[] = [];
  currentUser: any = null;
  errorMessage: string = '';
  displayedColumns: string[] = ['policyNumber', 'planName', 'status', 'premium', 'actions'];

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
    private paymentService: PaymentService,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private dialog: MatDialog
  ) {
    this.currentUser = this.authService.currentUser;
  }

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
    this.policyService.getPagedPolicies(this.pageIndex + 1, this.pageSize, this.sortColumn, this.isAscending, this.searchTerm).subscribe({
      next: (response) => {
        this.policies = response.items;
        this.totalCount = response.totalCount;
        this.errorMessage = '';
        this.cdr.detectChanges();
      },
      error: () => {
        this.errorMessage = 'Unable to load policies at this time.';
        this.cdr.detectChanges();
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

    this.sortColumn = event.active;
    if (this.sortColumn === 'policyNumber') this.sortColumn = 'PolicyNumber';
    if (this.sortColumn === 'planName') this.sortColumn = 'PlanId';
    if (this.sortColumn === 'status') this.sortColumn = 'Status';
    if (this.sortColumn === 'premium') this.sortColumn = 'PolicyId';

    this.isAscending = event.direction === 'asc';
    this.pageIndex = 0;
    this.loadPolicies();
  }

  payPremium(policy: any) {
    const dialogRef = this.dialog.open(PaymentDialog, {
      width: '400px',
      data: { amount: policy.premiumAmount, policyNumber: policy.policyNumber }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {

        const paymentDto = {
          policyId: policy.policyId,
          amount: policy.premiumAmount,
          paymentMethod: result.paymentMode
        };

        this.paymentService.payPremium(paymentDto).subscribe({
          next: () => {
            alert('Payment Successful!');
            this.loadPolicies();
          },
          error: (err) => alert('Payment failed: ' + (err.error?.message || err.message))
        });
      }
    });
  }

  initiateClaim(policy: any) {
    this.router.navigate(['/customer/submit-claim'], { queryParams: { policyId: policy.policyId } });
  }
}


