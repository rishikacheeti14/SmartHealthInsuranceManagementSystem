import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { Router } from '@angular/router';
import { finalize } from 'rxjs/operators';

import { TreatmentService } from '../../../core/services/treatment.service';
import { PolicyService } from '../../../core/services/policy.service';

@Component({
  selector: 'app-treatment-submission',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  providers: [MatDatepickerModule],
  templateUrl: './treatment-submission.html',
  styleUrls: ['./treatment-submission.css']
})
export class TreatmentSubmission implements OnInit {
  treatmentForm: FormGroup;
  verifiedPolicy: any = null;
  policyNumberSearch: string = '';
  isSearching: boolean = false;

  constructor(
    private fb: FormBuilder,
    private treatmentService: TreatmentService,
    private policyService: PolicyService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.treatmentForm = this.fb.group({
      policyId: ['', Validators.required],
      hospitalName: [{ value: 'Apollo Central', disabled: true }],
      startDate: [new Date(), Validators.required],
      endDate: [new Date()],
      condition: ['', Validators.required],
      treatment: ['', Validators.required],
      estimatedCost: ['', [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit() {
  }

  verifyPolicy() {
    if (!this.policyNumberSearch) return;
    this.isSearching = true;
    this.verifiedPolicy = null;
    this.treatmentForm.patchValue({ policyId: '' });



    this.policyService.getPolicyByNumber(this.policyNumberSearch)
      .pipe(finalize(() => {
        this.isSearching = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (policyDto: any) => {
          this.verifiedPolicy = policyDto;
          this.treatmentForm.patchValue({ policyId: policyDto.policyId });
          this.cdr.detectChanges();
        },
        error: (err: any) => {
          const msg = err.error?.message || err.statusText || 'Unknown Error';
          alert(`Verification Failed: ${msg}`);
          this.cdr.detectChanges();
        }
      });
  }

  onSubmit() {
    if (this.treatmentForm.valid && this.verifiedPolicy) {
      const formValue = this.treatmentForm.getRawValue();

      const payload = {
        PolicyNumber: this.verifiedPolicy.policyNumber, 
        Diagnosis: formValue.condition,               
        TreatmentDetails: formValue.treatment,         
        TreatmentDate: formValue.startDate,            
        TreatmentCost: Number(formValue.estimatedCost)  
      };

      this.treatmentService.submitTreatment(payload).subscribe({
        next: () => {
          alert('Treatment submitted successfully');
          this.router.navigate(['/hospital']);
        },
        error: (err) => {
          alert('Submission failed: ' + (err.error?.message || err.message));
        }
      });
    } else {
      if (!this.verifiedPolicy) alert('Please verify the policy first.');
    }
  }
}
