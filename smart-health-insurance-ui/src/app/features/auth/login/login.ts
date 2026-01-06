import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatInputModule,
    MatInputModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule
  ],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class Login {
  loginForm: FormGroup;
  hidePassword = true;


  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit() {
    if (this.loginForm.valid) {
      const formValue = this.loginForm.value;
      const credentials = {
        email: formValue.email?.trim() || '',
        password: formValue.password?.trim() || ''
      };
      this.authService.login(credentials).subscribe({
        next: () => {

          const role = this.authService.currentRole;
          if (role === 'Admin') {
            this.router.navigate(['/admin']);
          } else if (role === 'InsuranceAgent') {
            this.router.navigate(['/agent']);
          } else if (role === 'HospitalManager') {
            this.router.navigate(['/hospital']);
          } else if (role === 'Customer') {
            this.router.navigate(['/customer']);
          } else if (role === 'ClaimsOfficer') {
            this.router.navigate(['/claims']);
          } else {
            this.router.navigate(['/']);
          }
        },
        error: (err) => {
          alert('Login failed: ' + (err.error?.message || 'Invalid credentials'));
        }
      });
    }
  }
}
