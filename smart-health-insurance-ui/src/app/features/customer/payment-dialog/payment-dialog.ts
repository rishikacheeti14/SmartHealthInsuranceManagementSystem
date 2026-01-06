import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'app-payment-dialog',
    standalone: true,
    imports: [
        CommonModule,
        MatDialogModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatButtonModule,
        FormsModule
    ],
    templateUrl: './payment-dialog.html'
})
export class PaymentDialog {
    paymentMode: string = '';

    constructor(
        public dialogRef: MatDialogRef<PaymentDialog>,
        @Inject(MAT_DIALOG_DATA) public data: { amount: number, policyNumber: string }
    ) { }

    onCancel(): void {
        this.dialogRef.close();
    }

    onPay(): void {
        this.dialogRef.close({ paymentMode: this.paymentMode });
    }
}
