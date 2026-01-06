
import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'app-edit-claim-dialog',
    standalone: true,
    imports: [
        CommonModule,
        MatDialogModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        FormsModule
    ],
    template: `
    <h2 mat-dialog-title>Edit Claim Description</h2>
    <mat-dialog-content>
      <p>You can verify or update the initial description before the hospital processes it.</p>
      <mat-form-field appearance="fill" style="width: 100%;">
        <mat-label>Treatment Description</mat-label>
        <textarea matInput [(ngModel)]="description" rows="4"></textarea>
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button color="primary" [mat-dialog-close]="description" [disabled]="!description">Save</button>
    </mat-dialog-actions>
  `
})
export class EditClaimDialog {
    description: string;

    constructor(
        public dialogRef: MatDialogRef<EditClaimDialog>,
        @Inject(MAT_DIALOG_DATA) public data: { description: string }
    ) {
        this.description = data.description;
    }
}
