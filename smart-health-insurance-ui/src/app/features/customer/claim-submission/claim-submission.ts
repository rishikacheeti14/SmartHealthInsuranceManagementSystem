import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { ClaimService } from '../../../core/services/claim.service';
import { HospitalService } from '../../../core/services/hospital.service';
import { PolicyService } from '../../../core/services/policy.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-claim-submission',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule
  ],
  templateUrl: './claim-submission.html',
  styleUrls: ['./claim-submission.css']
})
export class ClaimSubmission implements OnInit {
  claimForm: FormGroup;
  hospitals: any[] = [];

  constructor(
    private fb: FormBuilder,
    private claimService: ClaimService,
    private hospitalService: HospitalService,
    private policyService: PolicyService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.claimForm = this.fb.group({
      policyId: ['', Validators.required],
      hospitalId: ['', Validators.required],
      treatmentDescription: ['', [Validators.required, Validators.minLength(10)]]
    });
  }

  policies: any[] = [];

  ngOnInit() {
    this.loadData();
  }

  loadData() {

    this.hospitalService.getHospitals().subscribe({
      next: (data) => {
        this.hospitals = data;
        this.cdr.detectChanges();
      },
      error: () => { }
    });

    this.policyService.getPagedPolicies(1, 100, 'PolicyId', false).subscribe({
      next: (res) => {
        this.policies = (res.items || []).filter((p: any) => p.status === 'Active');
        this.cdr.detectChanges();
      }
    });
  }

  onSubmit() {
    if (this.claimForm.valid) {
      this.claimService.initiateClaim(this.claimForm.value).subscribe({
        next: () => {
          alert('Claim initiated successfully! Please wait for the hospital to provide treatment details.');
          this.claimForm.reset();
          Object.keys(this.claimForm.controls).forEach(key => {
            this.claimForm.get(key)?.setErrors(null);
          });
          window.scrollTo(0, 0);
        },
        error: (err) => alert('Initiation failed: ' + (err.error?.message || err.message))
      });
    }
  }
}
