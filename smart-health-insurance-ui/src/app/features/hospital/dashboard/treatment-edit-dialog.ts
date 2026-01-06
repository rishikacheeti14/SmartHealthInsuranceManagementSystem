import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';

@Component({
    selector: 'app-treatment-edit-dialog',
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
    <h2 mat-dialog-title>Edit Treatment</h2>
    <mat-dialog-content>
      <form [formGroup]="editForm" class="edit-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Diagnosis</mat-label>
          <input matInput formControlName="diagnosis">
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Treatment Details</mat-label>
          <textarea matInput formControlName="treatmentDetails" rows="3"></textarea>
        </mat-form-field>

        <div class="row">
          <mat-form-field appearance="outline">
            <mat-label>Cost (INR)</mat-label>
            <input matInput type="number" formControlName="treatmentCost">
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Treatment Date</mat-label>
            <input matInput [matDatepicker]="picker" formControlName="treatmentDate">
            <mat-datepicker-toggle matIconSuffix [for]="picker"></mat-datepicker-toggle>
            <mat-datepicker #picker></mat-datepicker>
          </mat-form-field>
        </div>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" (click)="onSave()" [disabled]="editForm.invalid">Save Changes</button>
    </mat-dialog-actions>
  `,
    styles: [`
    .edit-form { display: flex; flex-direction: column; gap: 10px; padding-top: 10px; min-width: 400px; }
    .full-width { width: 100%; }
    .row { display: flex; gap: 15px; }
    mat-form-field { flex: 1; }
  `]
})
export class TreatmentEditDialog {
    editForm: FormGroup;

    constructor(
        private fb: FormBuilder,
        public dialogRef: MatDialogRef<TreatmentEditDialog>,
        @Inject(MAT_DIALOG_DATA) public data: any
    ) {
        this.editForm = this.fb.group({
            diagnosis: [data.diagnosis, Validators.required],
            treatmentDetails: [data.treatmentDetails || '', Validators.required],
            treatmentCost: [data.treatmentCost, [Validators.required, Validators.min(0)]],
            treatmentDate: [new Date(data.treatmentDate), Validators.required]
        });
    }

    onCancel(): void {
        this.dialogRef.close();
    }

    onSave(): void {
        if (this.editForm.valid) {
            this.dialogRef.close(this.editForm.value);
        }
    }
}
