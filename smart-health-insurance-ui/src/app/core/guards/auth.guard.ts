import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
    providedIn: 'root'
})
export class AuthGuard implements CanActivate {
    constructor(private auth: AuthService, private router: Router) { }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
        if (!this.auth.isAuthenticated()) {
            this.router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
            return false;
        }

        const expectedRoles = route.data['roles'] as Array<string>;
        if (expectedRoles) {
            const userRole = this.auth.currentRole;
            if (!userRole || !expectedRoles.includes(userRole)) {
                alert('Access Denied: You do not have permission to view this page.');
                this.router.navigate(['/']);
                return false;
            }
        }

        return true;
    }
}
