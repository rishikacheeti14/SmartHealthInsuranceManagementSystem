import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { PolicyService } from '../../../core/services/policy.service';

@Component({
    selector: 'app-edit-policy-dialog',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatDatepickerModule,
        MatNativeDateModule
    ],
    templateUrl: './edit-policy-dialog.component.html',
    styles: [`
    form { display: flex; flex-direction: column; gap: 15px; }
  `]
})
export class EditPolicyDialogComponent implements OnInit {
    editForm: FormGroup;
    plans: any[] = [];

    constructor(
        private fb: FormBuilder,
        private policyService: PolicyService,
        public dialogRef: MatDialogRef<EditPolicyDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: any
    ) {
        this.editForm = this.fb.group({
            planId: [data.planId, Validators.required],
            startDate: [data.startDate, Validators.required]
        });
    }

    ngOnInit(): void {
        this.policyService.getAllPlans().subscribe(plans => {
            this.plans = plans;
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
