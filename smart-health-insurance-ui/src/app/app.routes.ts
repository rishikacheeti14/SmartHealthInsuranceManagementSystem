import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },
  {
    path: 'auth',
    children: [
      {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login').then(m => m.Login)
      },
      {
        path: 'register',
        loadComponent: () => import('./features/auth/register/register').then(m => m.Register)
      }
    ]
  },
  {
    path: 'admin',
    canActivate: [() => import('./core/guards/auth.guard').then(m => m.AuthGuard)],
    data: { roles: ['Admin'] },
    children: [
      { path: '', loadComponent: () => import('./features/admin/dashboard/dashboard').then(m => m.Dashboard) },
      { path: 'users', loadComponent: () => import('./features/admin/user-management/user-management').then(m => m.UserManagement) },
      { path: 'plans', loadComponent: () => import('./features/admin/plan-management/plan-management').then(m => m.PlanManagement) },
      { path: 'hospitals', loadComponent: () => import('./features/admin/hospital-management/hospital-management').then(m => m.HospitalManagement) }
    ]
  },
  {
    path: 'agent',
    canActivate: [() => import('./core/guards/auth.guard').then(m => m.AuthGuard)],
    data: { roles: ['InsuranceAgent'] },
    children: [
      { path: '', loadComponent: () => import('./features/agent/dashboard/dashboard').then(m => m.Dashboard) },
      { path: 'enroll', loadComponent: () => import('./features/agent/policy-enrollment/policy-enrollment').then(m => m.PolicyEnrollment) },
      { path: 'my-enrollments', loadComponent: () => import('./features/agent/my-enrollments/my-enrollments.component').then(m => m.MyEnrollmentsComponent) }
    ]
  },
  {
    path: 'hospital',
    canActivate: [() => import('./core/guards/auth.guard').then(m => m.AuthGuard)],
    data: { roles: ['HospitalManager'] },
    children: [
      { path: '', loadComponent: () => import('./features/hospital/dashboard/dashboard').then(m => m.Dashboard) },
      { path: 'submit-treatment', loadComponent: () => import('./features/hospital/treatment-submission/treatment-submission').then(m => m.TreatmentSubmission) }
    ]
  },
  {
    path: 'customer',
    canActivate: [() => import('./core/guards/auth.guard').then(m => m.AuthGuard)],
    data: { roles: ['Customer'] },
    children: [
      { path: '', loadComponent: () => import('./features/customer/dashboard/dashboard').then(m => m.Dashboard) },
      { path: 'policies', loadComponent: () => import('./features/customer/my-policies/my-policies').then(m => m.MyPolicies) },
      { path: 'submit-claim', loadComponent: () => import('./features/customer/claim-submission/claim-submission').then(m => m.ClaimSubmission) },
      { path: 'claims', loadComponent: () => import('./features/customer/my-claims/my-claims').then(m => m.MyClaims) }
    ]
  },
  {
    path: 'claims',
    canActivate: [() => import('./core/guards/auth.guard').then(m => m.AuthGuard)],
    data: { roles: ['ClaimsOfficer'] },
    children: [
      { path: '', loadComponent: () => import('./features/claims-officer/dashboard/dashboard').then(m => m.Dashboard) },
      { path: 'review', loadComponent: () => import('./features/claims-officer/claim-review/claim-review').then(m => m.ClaimReview) },
      { path: 'archived', loadComponent: () => import('./features/claims-officer/archived/archived-claims').then(m => m.ArchivedClaims) }
    ]
  }
];
