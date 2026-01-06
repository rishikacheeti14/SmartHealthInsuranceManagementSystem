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
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort, MatSort } from '@angular/material/sort';
import { UserService } from '../../../core/services/user.service';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

@Component({
  selector: 'app-user-management',
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
    MatPaginatorModule,
    MatSortModule
  ],
  templateUrl: './user-management.html',
  styleUrls: ['./user-management.css']
})
export class UserManagement implements OnInit, OnDestroy {
  @ViewChild(MatSort) sort!: MatSort;
  users: any[] = [];
  displayedColumns: string[] = ['id', 'name', 'email', 'role', 'status', 'actions'];
  showCreateForm = false;
  staffForm: FormGroup;
  roles = ['InsuranceAgent', 'HospitalManager', 'ClaimsOfficer', 'Admin'];

  totalCount = 0;
  pageSize = 5;
  pageIndex = 0;
  sortColumn = 'UserId';
  isAscending = false;

  searchTerm = '';
  private searchTermSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  constructor(private userService: UserService, private fb: FormBuilder, private cdr: ChangeDetectorRef) {
    this.staffForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email, Validators.pattern('^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,4}$')]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      role: ['', Validators.required]
    });
  }

  ngOnInit() {
    this.loadUsers();

    this.searchTermSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(term => {
      this.searchTerm = term;
      this.pageIndex = 0; 
      this.loadUsers();
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearch(term: string) {
    this.searchTermSubject.next(term);
  }

  loadUsers() {
    this.userService.getPagedUsers(this.pageIndex + 1, this.pageSize, this.sortColumn, this.isAscending, this.searchTerm).subscribe({
      next: (response) => {
        const data = response.items;
        this.totalCount = response.totalCount;

        this.users = data.map((u: any) => ({
          userId: u.userId || u.UserId,
          firstName: u.firstName || u.FirstName,
          lastName: u.lastName || u.LastName,
          email: u.email || u.Email,
          role: u.role || u.Role,
          isActive: (u.isActive !== undefined) ? u.isActive : u.IsActive
        }));

        this.cdr.detectChanges();
      },
      error: () => { }
    });
  }

  onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadUsers();
  }

  onSortChange(event: Sort) {
    if (event.direction === 'desc') {
      this.sort.active = '';
      this.sort.direction = '';
      this.cdr.detectChanges();
      this.sortColumn = 'UserId';
      this.isAscending = false;
      this.pageIndex = 0;
      this.loadUsers();
      return;
    }

    this.sortColumn = event.active;
    if (this.sortColumn === 'name') this.sortColumn = 'FirstName';
    if (this.sortColumn === 'id') this.sortColumn = 'UserId';

    this.isAscending = event.direction === 'asc';
    this.pageIndex = 0;
    this.loadUsers();
  }

  editingUserId: number | null = null;

  toggleForm() {
    this.showCreateForm = !this.showCreateForm;
    if (!this.showCreateForm) {
      this.resetForm();
    }
  }

  resetForm() {
    this.editingUserId = null;
    this.staffForm.reset();
    this.staffForm.get('password')?.setValidators([Validators.required, Validators.minLength(6)]);
    this.staffForm.get('password')?.updateValueAndValidity();
  }

  startEdit(user: any) {
    this.editingUserId = user.userId;
    this.showCreateForm = true;

    this.staffForm.patchValue({
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      role: user.role,
      password: '' 
    });

    this.staffForm.get('password')?.clearValidators();
    this.staffForm.get('password')?.updateValueAndValidity();

  }

  saveUser() { 
    if (this.staffForm.valid) {
      const userData = { ...this.staffForm.value };

      Object.keys(userData).forEach(key => {
        if (typeof userData[key] === 'string') {
          userData[key] = userData[key].trim();
        }
      });

      if (this.editingUserId) {

        this.userService.updateUser(this.editingUserId, userData).subscribe({
          next: () => {
            alert('User updated successfully');
            this.loadUsers();
            this.toggleForm();
          },
          error: (err) => alert('Error: ' + (err.error?.message || err.message))
        });
      } else {
        this.userService.createStaff(userData).subscribe({
          next: () => {
            alert('Staff created successfully');
            this.loadUsers();
            this.toggleForm();
          },
          error: (err) => {
            const msg = err.error?.message || (typeof err.error === 'string' ? err.error : 'Unknown error');
            alert('Error: ' + msg);
          }
        });
      }
    }
  }

  toggleStatus(user: any) {
    if (confirm(`Are you sure you want to ${user.isActive ? 'deactivate' : 'activate'} this user?`)) {
      this.userService.toggleStatus(user.userId, user.isActive).subscribe({
        next: () => {
          this.loadUsers();
        },
        error: (err) => alert('Error updating status: ' + (err.error?.message || 'Unknown error'))
      });
    }
  }

  deleteUser(user: any) {
    if (confirm(`Are you sure you want to permanently delete ${user.email}? This action cannot be undone.`)) {
      this.userService.deleteUser(user.userId).subscribe({
        next: () => {
          alert('User deleted successfully');
          this.loadUsers();
        },
        error: (err) => alert('Error deleting user: ' + (err.error?.message || 'Unknown error'))
      });
    }
  }
}
