
import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-claim-details-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Claim Details #{{data.claimNumber}}</h2>
    <mat-dialog-content>
      <div class="details-grid">
        <p><strong>Status:</strong> {{data.status}}</p>
        <p><strong>Amount:</strong> {{data.claimAmount | currency:'INR'}}</p>
        <p><strong>Date:</strong> {{data.submittedAt | date}}</p>
        <p><strong>Hospital:</strong> {{data.hospitalName || 'N/A'}}</p>
        <p><strong>Diagnosis:</strong> {{data.treatmentRecord?.diagnosis || data.diagnosis || 'N/A'}}</p>
        <p><strong>Treatment:</strong> {{data.treatmentRecord?.treatmentDetails || data.treatmentDetails || data.treatmentDescription || 'N/A'}}</p>
        <div *ngIf="data.rejectionReason" class="rejection-box">
             <strong>Rejection Reason:</strong> {{data.rejectionReason}}
        </div>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Close</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .details-grid p { margin: 8px 0; }
    .rejection-box { 
        margin-top: 15px; 
        padding: 10px; 
        background-color: #fee2e2; 
        color: #b91c1c; 
        border-radius: 4px; 
    }
  `]
})
export class ClaimDetailsDialog {
  constructor(
    public dialogRef: MatDialogRef<ClaimDetailsDialog>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) { }
}
