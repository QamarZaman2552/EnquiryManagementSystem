import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable, tap, of } from 'rxjs';

interface LoginResponse {
  username: string;
  role: string;
}

interface MeResponse {
  username: string;
  role: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
    private baseUrl = environment.baseUrl;
    private _username: string | null = null;
    private _role: string | null = null;

    constructor(private http: HttpClient) { }

    login(username: string, password: string): Observable<LoginResponse> {
        return this.http.post<LoginResponse>(`${this.baseUrl}/Auth/login`, { username, password }, { withCredentials: true }).pipe(
            tap((res) => {
                this._username = res.username;
                this._role = res.role;
            })
        );
    }

    logout(): Observable<any> {
        return this.http.post(`${this.baseUrl}/Auth/logout`, {}, { withCredentials: true }).pipe(
            tap(() => this.clearSession())
        );
    }

    me(): Observable<MeResponse> {
        return this.http.get<MeResponse>(`${this.baseUrl}/Auth/me`, { withCredentials: true }).pipe(
            tap((res) => {
                this._username = res.username;
                this._role = res.role;
            })
        );
    }

    refreshToken(): Observable<LoginResponse> {
        return this.http.post<LoginResponse>(`${this.baseUrl}/Auth/refresh`, {}, { withCredentials: true }).pipe(
            tap((res) => {
                this._username = res.username;
                this._role = res.role;
            })
        );
    }

    isLoggedIn(): boolean {
        return this._username !== null;
    }

    isAdmin(): boolean {
        return this._role === 'Admin';
    }

    getUsername(): string | null {
        return this._username;
    }

    getRole(): string | null {
        return this._role;
    }

    restoreSession(): Promise<void> {
        if (this.isLoggedIn()) return Promise.resolve();
        return new Promise((resolve) => {
            this.me().subscribe({
                next: () => resolve(),
                error: () => {
                    this.clearSession();
                    resolve();
                }
            });
        });
    }

    clearSession(): void {
        this._username = null;
        this._role = null;
    }
}
