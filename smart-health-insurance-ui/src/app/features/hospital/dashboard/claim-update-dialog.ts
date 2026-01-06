import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';

@Component({
    selector: 'app-claim-update-dialog',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatDatepickerModule,
        MatNativeDateModule
    ],
    template: `
    <h2 mat-dialog-title>Provide Treatment Details</h2>
    <mat-dialog-content>
      <form [formGroup]="updateForm">
        <p><strong>Patient:</strong> {{data.customerName || data.userId}}</p>
        <p><strong>Request:</strong> {{data.treatmentDescription}}</p>
        
        <mat-form-field appearance="fill" class="full-width">
          <mat-label>Diagnosis *</mat-label>
          <input matInput formControlName="diagnosis">
        </mat-form-field>

        <mat-form-field appearance="fill" class="full-width">
          <mat-label>Treatment Cost (₹) *</mat-label>
          <input matInput type="number" formControlName="treatmentCost">
        </mat-form-field>

        <mat-form-field appearance="fill" class="full-width">
          <mat-label>Treatment Date *</mat-label>
          <input matInput [matDatepicker]="picker" formControlName="treatmentDate">
          <mat-datepicker-toggle matSuffix [for]="picker"></mat-datepicker-toggle>
          <mat-datepicker #picker></mat-datepicker>
        </mat-form-field>

        <mat-form-field appearance="fill" class="full-width">
          <mat-label>Treatment Details *</mat-label>
          <textarea matInput formControlName="treatmentDetails" rows="3"></textarea>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" [disabled]="updateForm.invalid" (click)="onSubmit()">Submit Details</button>
    </mat-dialog-actions>
  `,
    styles: [`
    .full-width { width: 100%; margin-bottom: 10px; }
  `]
})
export class ClaimUpdateDialog {
    updateForm: FormGroup;

    constructor(
        private fb: FormBuilder,
        public dialogRef: MatDialogRef<ClaimUpdateDialog>,
        @Inject(MAT_DIALOG_DATA) public data: any
    ) {
        this.updateForm = this.fb.group({
            diagnosis: ['', Validators.required],
            treatmentCost: ['', [Validators.required, Validators.min(0)]],
            treatmentDate: [new Date(), Validators.required],
            treatmentDetails: ['', Validators.required]
        });
    }

    onCancel(): void {
        this.dialogRef.close();
    }

    onSubmit(): void {
        if (this.updateForm.valid) {
            this.dialogRef.close(this.updateForm.value);
        }
    }
}
