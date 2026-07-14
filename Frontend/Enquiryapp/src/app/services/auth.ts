import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable, tap } from 'rxjs';
import { LoginResponse } from '../models/interfaces';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private baseUrl = environment.baseUrl;

    constructor(private http: HttpClient) { }

    login(username: string, password: string): Observable<LoginResponse> {
        return this.http.post<LoginResponse>(`${this.baseUrl}/Auth/login`, { username, password }).pipe(
            tap((res) => {
                localStorage.setItem('token', res.token);
                localStorage.setItem('username', res.username);
                localStorage.setItem('role', res.role);
            })
        );
    }

    logout(): void {
        localStorage.removeItem('token');
        localStorage.removeItem('username');
        localStorage.removeItem('role');
    }

    isLoggedIn(): boolean {
        return !!localStorage.getItem('token');
    }

    isAdmin(): boolean {
        return this.isLoggedIn() && localStorage.getItem('role') === 'Admin';
    }

    getToken(): string | null {
        return localStorage.getItem('token');
    }

    getUsername(): string | null {
        return localStorage.getItem('username');
    }
}
