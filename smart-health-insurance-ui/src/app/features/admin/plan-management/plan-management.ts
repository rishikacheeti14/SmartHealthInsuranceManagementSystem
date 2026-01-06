import { Component, OnInit, ChangeDetectorRef, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort, MatSort } from '@angular/material/sort';
import { PlanService } from '../../../core/services/plan.service';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

@Component({
    selector: 'app-plan-management',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatTableModule,
        MatButtonModule,
        MatCardModule,
        MatFormFieldModule,
        MatInputModule,
        MatIconModule,
        MatSlideToggleModule,
        MatPaginatorModule,
        MatSortModule
    ],
    templateUrl: './plan-management.html',
    styleUrls: ['./plan-management.css']
})
export class PlanManagement implements OnInit, OnDestroy {
    @ViewChild(MatSort) sort!: MatSort;
    plans: any[] = [];
    displayedColumns: string[] = ['name', 'premium', 'coverage', 'duration', 'status', 'actions'];
    showForm = false;
    isEditing = false;
    currentPlanId: number | null = null;
    planForm: FormGroup;

    totalCount = 0;
    pageSize = 5;
    pageIndex = 0;
    sortColumn = 'PlanId';
    isAscending = false;

    searchTerm = '';
    private searchTermSubject = new Subject<string>();
    private destroy$ = new Subject<void>();

    constructor(private planService: PlanService, private fb: FormBuilder, private cdr: ChangeDetectorRef) {
        this.planForm = this.fb.group({
            planName: ['', Validators.required],
            description: [''],
            premiumAmount: ['', [Validators.required, Validators.min(0)]],
            coverageLimit: ['', [Validators.required, Validators.min(0)]],
            durationInMonths: ['', [Validators.required, Validators.min(1)]],
            isActive: [true]
        });
    }

    ngOnInit() {
        this.loadPlans();

        this.searchTermSubject.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            takeUntil(this.destroy$)
        ).subscribe(term => {
            this.searchTerm = term;
            this.pageIndex = 0;
            this.loadPlans();
        });
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }

    onSearch(term: string) {
        this.searchTermSubject.next(term);
    }

    loadPlans() {
        this.planService.getPagedPlans(this.pageIndex + 1, this.pageSize, this.sortColumn, this.isAscending, this.searchTerm).subscribe({
            next: (response) => {
                const data = response.items;
                this.totalCount = response.totalCount;

                this.plans = data.map((p: any) => ({
                    planId: p.planId || p.PlanId,
                    planName: p.planName || p.PlanName,
                    description: p.description || p.Description,
                    premiumAmount: p.premiumAmount || p.PremiumAmount,
                    coverageLimit: p.coverageLimit || p.CoverageLimit,
                    durationInMonths: p.durationInMonths || p.DurationInMonths,
                    isActive: (p.isActive !== undefined) ? p.isActive : p.IsActive
                }));

                this.cdr.detectChanges();
            },
            error: () => { }
        });
    }

    onPageChange(event: PageEvent) {
        this.pageIndex = event.pageIndex;
        this.pageSize = event.pageSize;
        this.loadPlans();
    }

    onSortChange(event: Sort) {
        if (event.direction === 'desc') {
            this.sort.active = '';
            this.sort.direction = '';
            this.cdr.detectChanges();
            this.sortColumn = 'PlanId'; 
            this.isAscending = false;
            this.pageIndex = 0;
            this.loadPlans();
            return;
        }

        this.sortColumn = event.active;
        if (this.sortColumn === 'name') this.sortColumn = 'PlanName';
        if (this.sortColumn === 'premium') this.sortColumn = 'PremiumAmount';
        if (this.sortColumn === 'coverage') this.sortColumn = 'CoverageLimit';
        if (this.sortColumn === 'duration') this.sortColumn = 'DurationInMonths';

        this.isAscending = event.direction === 'asc';
        this.pageIndex = 0;
        this.loadPlans();
    }

    toggleForm() {
        this.showForm = !this.showForm;
        if (!this.showForm) {
            this.resetForm();
        }
    }

    resetForm() {
        this.planForm.reset({ isActive: true });
        this.isEditing = false;
        this.currentPlanId = null;
    }

    onSubmit() {
        if (this.planForm.valid) {
            const planData = this.planForm.value;

            if (this.isEditing && this.currentPlanId) {
                this.planService.updatePlan(this.currentPlanId, planData).subscribe({
                    next: () => {
                        alert('Plan updated successfully');
                        this.loadPlans();
                        this.toggleForm();
                    },
                    error: (err) => alert('Error updating plan: ' + (err.error?.message || err.message))
                });
            } else {
                this.planService.createPlan(planData).subscribe({
                    next: () => {
                        alert('Plan created successfully');
                        this.loadPlans();
                        this.toggleForm();
                    },
                    error: (err) => alert('Error creating plan: ' + (err.error?.message || err.message))
                });
            }
        }
    }

    editPlan(plan: any) {

        this.isEditing = true;
        this.currentPlanId = plan.planId;

        this.planForm.patchValue({
            planName: plan.planName,
            description: plan.description,
            premiumAmount: plan.premiumAmount,
            coverageLimit: plan.coverageLimit,
            durationInMonths: plan.durationInMonths,
            isActive: plan.isActive
        });


        this.showForm = true;
        this.cdr.detectChanges();
    }

    deletePlan(plan: any) {
        if (confirm(`Are you sure you want to delete ${plan.planName}?`)) {
            this.planService.deletePlan(plan.planId).subscribe({
                next: () => {
                    alert('Plan deleted successfully');
                    this.loadPlans();
                },
                error: (err) => alert('Error deleting plan: ' + (err.error?.message || err.message))
            });
        }
    }
}
