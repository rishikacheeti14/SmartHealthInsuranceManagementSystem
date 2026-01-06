import { Component, Inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { PolicyService } from '../../../core/services/policy.service';

@Component({
  selector: 'app-policy-selection-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule
  ],
  template: `
    <h2 mat-dialog-title>Select Policy to Finalize Claim</h2>
    <mat-dialog-content>
      <div *ngIf="!isLoading && policies.length === 0" style="padding: 10px; color: #f44336;">
        No active and paid policies found. You need an active, paid policy to finalize a claim.
      </div>

      <div *ngIf="isLoading" style="padding: 10px; color: gray;">
        Loading policies...
      </div>

      <form [formGroup]="policyForm" *ngIf="policies.length > 0">
        <p><strong>Claim ID:</strong> {{data.claimNumber}}</p>
        <p><strong>Treatment:</strong> {{data.treatmentDetails || data.treatmentDescription || 'Details pending'}}</p>
        <p><strong>Cost:</strong> {{data.claimAmount | currency:'INR'}}</p>

        <mat-form-field appearance="fill" class="full-width">
          <mat-label>Select Active Policy *</mat-label>
          <mat-select formControlName="policyId">
            <mat-option *ngFor="let p of policies" [value]="p.policyId">
              {{p.policyNumber}} - {{p.plan?.planName}} (Ends: {{p.endDate | date}})
            </mat-option>
          </mat-select>
          <mat-error *ngIf="policyForm.get('policyId')?.hasError('required')">Required</mat-error>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" 
        [disabled]="policyForm.invalid || policies.length === 0" 
        (click)="onSubmit()">Submit Claim</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width { width: 100%; margin-top: 15px; }
  `]
})
export class PolicySelectionDialog implements OnInit {
  policyForm: FormGroup;
  policies: any[] = [];
  isLoading = true;

  constructor(
    private fb: FormBuilder,
    private policyService: PolicyService,
    private cdr: ChangeDetectorRef,
    public dialogRef: MatDialogRef<PolicySelectionDialog>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.policyForm = this.fb.group({
      policyId: ['', Validators.required]
    });
  }

  ngOnInit() {
    this.policyService.getMyPolicies().subscribe({
      next: (data) => {
        this.policies = data.filter((p: any) => (p.status === 'Active' || p.status === 1) && p.premiumPaid === true);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSubmit(): void {
    if (this.policyForm.valid) {
      this.dialogRef.close(this.policyForm.value);
    }
  }
}
