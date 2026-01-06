import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';

@Component({
  selector: 'app-treatment-details-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatListModule
  ],
  template: `
    <h2 mat-dialog-title>Treatment Details</h2>
    <mat-dialog-content>
      <div class="details-container">
        <div class="detail-item">
          <span class="label">Patient:</span>
          <span class="value">{{ data.customerName || 'Unknown' }}</span>
        </div>
        <div class="detail-item">
          <span class="label">Diagnosis:</span>
          <span class="value">{{ data.diagnosis }}</span>
        </div>
        <div class="detail-item">
          <span class="label">Cost:</span>
          <span class="value">{{ data.treatmentCost | currency:'INR' }}</span>
        </div>
        <div class="detail-item full-width">
          <span class="label">Details:</span>
          <p class="value description">{{ data.treatmentDetails || 'No additional details provided.' }}</p>
        </div>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onClose()">Close</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .details-container { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; min-width: 450px; padding-top: 10px; }
    .detail-item { display: flex; flex-direction: column; }
    .full-width { grid-column: 1 / -1; }
    .label { font-weight: bold; color: #555; font-size: 0.9rem; }
    .value { font-size: 1rem; color: #000; margin-top: 4px; }
    .description { background: #f5f5f5; padding: 10px; border-radius: 4px; white-space: pre-wrap; }
  `]
})
export class TreatmentDetailsDialog {
  constructor(
    public dialogRef: MatDialogRef<TreatmentDetailsDialog>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) { }

  onClose(): void {
    this.dialogRef.close();
  }
}
