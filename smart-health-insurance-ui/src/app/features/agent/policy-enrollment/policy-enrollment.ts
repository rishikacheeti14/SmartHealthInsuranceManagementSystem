import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormGroupDirective } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { PolicyService } from '../../../core/services/policy.service';
import { UserService } from '../../../core/services/user.service';

@Component({
  selector: 'app-policy-enrollment',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSnackBarModule
  ],
  providers: [MatDatepickerModule],
  templateUrl: './policy-enrollment.html',
  styleUrls: ['./policy-enrollment.css']
})
export class PolicyEnrollment implements OnInit {
  enrollForm: FormGroup;
  plans: any[] = [];
  customers: any[] = [];

  @ViewChild(FormGroupDirective) formDirective!: FormGroupDirective;

  constructor(
    private fb: FormBuilder,
    private policyService: PolicyService,
    private snackBar: MatSnackBar
  ) {
    this.enrollForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      dob: ['', Validators.required],
      planId: ['', Validators.required],
      startDate: [new Date(), Validators.required]
    });
  }

  ngOnInit() {
    this.loadPlans();
  }

  loadPlans() {
    this.policyService.getAllPlans().subscribe({
      next: (data) => this.plans = data,
      error: (err) => console.error('Error loading plans', err)
    });
  }

  onSubmit() {
    if (this.enrollForm.valid) {
      const enrollData = { ...this.enrollForm.value };
      // Trim string fields
      enrollData.firstName = enrollData.firstName.trim();
      enrollData.lastName = enrollData.lastName.trim();
      enrollData.email = enrollData.email.trim();

      this.policyService.enrollPolicy(enrollData).subscribe({
        next: (res) => {
          this.snackBar.open(res.message || 'Policy enrolled successfully!', 'Close', {
            duration: 5000,
            horizontalPosition: 'end',
            verticalPosition: 'top',
            panelClass: ['success-snackbar']
          });
          this.formDirective.resetForm();
          this.enrollForm.reset({ startDate: new Date() });
          this.enrollForm.patchValue({ startDate: new Date() });
        },
        error: (err) => {
          this.snackBar.open('Enrollment failed: ' + (err.error?.message || err.message), 'Close', {
            duration: 5000,
            horizontalPosition: 'end',
            verticalPosition: 'top',
            panelClass: ['error-snackbar']
          });
        }
      });
    }
  }
}
