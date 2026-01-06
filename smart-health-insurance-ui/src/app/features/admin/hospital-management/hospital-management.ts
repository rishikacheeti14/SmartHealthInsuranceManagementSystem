import { Component, OnInit, ChangeDetectorRef, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort, MatSort } from '@angular/material/sort';
import { HospitalService } from '../../../core/services/hospital.service';
import { UserService } from '../../../core/services/user.service';
import { INDIAN_STATES_CITIES } from '../../../core/data/indian-locations';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

@Component({
    selector: 'app-hospital-management',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatTableModule,
        MatButtonModule,
        MatCardModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatIconModule,
        MatSlideToggleModule,
        MatPaginatorModule,
        MatSortModule
    ],
    templateUrl: './hospital-management.html',
    styleUrls: ['./hospital-management.css']
})
export class HospitalManagement implements OnInit, OnDestroy {
    @ViewChild(MatSort) sort!: MatSort;
    hospitals: any[] = [];
    managers: any[] = [];
    displayedColumns: string[] = ['name', 'city', 'phone', 'network', 'actions'];
    showForm = false;
    isEditing = false;
    currentHospitalId: number | null = null;
    hospitalForm: FormGroup;

    states = Object.keys(INDIAN_STATES_CITIES);
    cities: string[] = [];

    // Pagination & Sorting State
    totalCount = 0;
    pageSize = 5;
    pageIndex = 0;
    sortColumn = 'HospitalId';
    isAscending = false;

    // Search State
    searchTerm = '';
    private searchTermSubject = new Subject<string>();
    private destroy$ = new Subject<void>();

    constructor(
        private hospitalService: HospitalService,
        private userService: UserService,
        private fb: FormBuilder,
        private cdr: ChangeDetectorRef
    ) {
        this.hospitalForm = this.fb.group({
            hospitalName: ['', Validators.required],
            address: ['', Validators.required],
            state: ['', Validators.required],
            city: ['', Validators.required],
            zipCode: ['', [Validators.required, Validators.pattern('^[0-9]{6}$')]], // Indian Zip Code
            phoneNumber: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
            email: ['', [Validators.required, Validators.email, Validators.pattern('^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,4}$')]],
            userId: ['', Validators.required], // Selected Manager ID
            isNetworkProvider: [true]
        });

        // Cascading Dropdown Logic
        this.hospitalForm.get('state')?.valueChanges.subscribe(selectedState => {
            this.cities = INDIAN_STATES_CITIES[selectedState] || [];
            this.hospitalForm.get('city')?.setValue(''); // Reset city
        });
    }

    // Manager Filtering State
    assignedManagerIds: Set<number> = new Set();
    currentManagerId: number | null = null;

    ngOnInit() {
        this.loadHospitals();
        this.loadManagers();
        this.loadAssignedManagers(); // Fetch all assignments

        // Setup Search
        this.searchTermSubject.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            takeUntil(this.destroy$)
        ).subscribe(term => {
            this.searchTerm = term;
            this.pageIndex = 0;
            this.loadHospitals();
        });
    }

    loadAssignedManagers() {
        this.hospitalService.getHospitals().subscribe({
            next: (data) => {
                this.assignedManagerIds = new Set(data.map(h => h.userId || h.UserId).filter(id => id > 0));
            }
        });
    }

    get availableManagers() {
        return this.managers.filter(m =>
            !this.assignedManagerIds.has(m.userId) ||
            (this.isEditing && this.currentManagerId === m.userId)
        );
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }

    onSearch(term: string) {
        this.searchTermSubject.next(term);
    }

    loadHospitals() {
        this.hospitalService.getPagedHospitals(this.pageIndex + 1, this.pageSize, this.sortColumn, this.isAscending, this.searchTerm).subscribe({
            next: (response) => {
                const data = response.items;
                this.totalCount = response.totalCount;

                this.hospitals = data.map((h: any) => ({
                    hospitalId: h.hospitalId || h.HospitalId,
                    hospitalName: h.hospitalName || h.HospitalName,
                    city: h.city || h.City,
                    state: h.state || h.State,
                    phoneNumber: h.phoneNumber || h.PhoneNumber,
                    email: h.email || h.Email,
                    isNetworkProvider: (h.isNetworkProvider !== undefined) ? h.isNetworkProvider : h.IsNetworkProvider
                }));

                // Refresh assignments too in case list changed significantly (though usually create/update does this)
                this.loadAssignedManagers();
                this.cdr.detectChanges();
            },
            error: () => { }
        });
    }

    onPageChange(event: PageEvent) {
        this.pageIndex = event.pageIndex;
        this.pageSize = event.pageSize;
        this.loadHospitals();
    }

    onSortChange(event: Sort) {
        if (event.direction === 'desc') {
            this.sort.active = '';
            this.sort.direction = '';
            this.cdr.detectChanges();
            this.sortColumn = 'HospitalId'; // Default
            this.isAscending = false;
            this.pageIndex = 0;
            this.loadHospitals();
            return;
        }

        this.sortColumn = event.active;
        if (this.sortColumn === 'name') this.sortColumn = 'HospitalName';
        if (this.sortColumn === 'phone') this.sortColumn = 'PhoneNumber';
        if (this.sortColumn === 'network') this.sortColumn = 'IsNetworkProvider';

        this.isAscending = event.direction === 'asc';
        this.pageIndex = 0;
        this.loadHospitals();
    }

    loadManagers() {
        this.userService.getAllUsers().subscribe({
            next: (data) => {
                // Filter only HospitalManagers
                this.managers = data
                    .filter((u: any) => (u.role === 'HospitalManager' || u.Role === 'HospitalManager'))
                    .map((u: any) => ({
                        userId: u.userId || u.UserId,
                        fullName: `${u.firstName || u.FirstName} ${u.lastName || u.LastName} (${u.email || u.Email})`
                    }));
            }
        });
    }

    toggleForm() {
        this.showForm = !this.showForm;
        if (!this.showForm) {
            this.resetForm();
        }
    }

    resetForm() {
        this.hospitalForm.reset({ isNetworkProvider: true });
        this.isEditing = false;
        this.currentHospitalId = null;
        this.currentManagerId = null;
    }

    onSubmit() {
        if (this.hospitalForm.valid) {
            const hospitalData = this.hospitalForm.value;

            if (this.isEditing && this.currentHospitalId) {
                this.hospitalService.updateHospital(this.currentHospitalId, hospitalData).subscribe({
                    next: () => {
                        alert('Hospital updated successfully');
                        this.loadHospitals();
                        this.toggleForm();
                    },
                    error: (err) => alert('Error updating hospital: ' + (err.error?.message || err.message))
                });
            } else {
                this.hospitalService.createHospital(hospitalData).subscribe({
                    next: () => {
                        alert('Hospital created successfully');
                        this.loadHospitals();
                        this.toggleForm();
                    },
                    error: (err) => alert('Error creating hospital: ' + (err.error?.message || err.message))
                });
            }
        }
    }

    editHospital(hospital: any) {
        this.isEditing = true;
        this.currentHospitalId = hospital.hospitalId;

        // For editing, we might need to fetch the full details since the list DTO is lightweight
        // But for now let's try patching what we have, realizing address/zip might be missing in list DTO.
        // Ideally we should call getHospitalById here.

        this.hospitalService.getHospitalById(hospital.hospitalId).subscribe((fullHospital: any) => {
            this.currentManagerId = fullHospital.userId || fullHospital.UserId; // Set current manager for filter exception
            this.hospitalForm.patchValue({
                hospitalName: fullHospital.hospitalName,
                address: fullHospital.address,
                city: fullHospital.city,
                state: fullHospital.state,
                zipCode: fullHospital.zipCode,
                phoneNumber: fullHospital.phoneNumber,
                email: fullHospital.email,
                userId: fullHospital.userId,
                isNetworkProvider: fullHospital.isNetworkProvider
            });
            this.showForm = true;
            this.cdr.detectChanges();
        });
    }

    deleteHospital(hospital: any) {
        if (confirm(`Are you sure you want to delete ${hospital.hospitalName}?`)) {
            this.hospitalService.deleteHospital(hospital.hospitalId).subscribe({
                next: () => {
                    alert('Hospital deleted successfully');
                    this.loadHospitals();
                },
                error: (err) => alert('Error deleting hospital: ' + (err.error?.message || err.message))
            });
        }
    }
}
