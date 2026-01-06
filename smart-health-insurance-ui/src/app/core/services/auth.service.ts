import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private tokenKey = 'token';
    private userSubject = new BehaviorSubject<any>(null);
    public user$ = this.userSubject.asObservable();

    constructor(private api: ApiService, private router: Router) {
        this.loadUser();
    }

    private loadUser() {
        const token = localStorage.getItem(this.tokenKey);
        if (token) {
            const decoded: any = jwtDecode(token);
            this.userSubject.next(decoded);
        }
    }

    get token(): string | null {
        return localStorage.getItem(this.tokenKey);
    }

    get currentUser(): any {
        return this.userSubject.value;
    }

    get currentRole(): string | null {
        const user = this.currentUser;
        return user ? (user['role'] || user['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']) : null;
    }

    login(credentials: any): Observable<any> {
        return this.api.post('Auth/login', credentials).pipe(
            tap((response: any) => {
                if (response && response.token) {
                    localStorage.setItem(this.tokenKey, response.token);
                    this.loadUser();
                }
            })
        );
    }

    registerCustomer(data: any): Observable<any> {
        return this.api.post('Auth/register', data);
    }

    logout() {
        localStorage.removeItem(this.tokenKey);
        this.userSubject.next(null);
        this.router.navigate(['/auth/login']);
    }

    isAuthenticated(): boolean {
        return !!this.token; 
    }
}
