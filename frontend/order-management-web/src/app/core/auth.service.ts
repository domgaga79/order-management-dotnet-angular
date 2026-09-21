import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { API } from './api';
import { AuthResponse } from './models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'om_token';
  private readonly userKey = 'om_user';

  constructor(private readonly http: HttpClient) {}

  login(email: string, password: string) {
    return this.http
      .post<AuthResponse>(`${API}/auth/login`, { email, password })
      .pipe(tap(response => this.saveSession(response)));
  }

  token(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  user(): AuthResponse | null {
    const raw = localStorage.getItem(this.userKey);
    if (!raw) return null;

    try {
      return JSON.parse(raw) as AuthResponse;
    } catch {
      this.logout();
      return null;
    }
  }

  isLoggedIn(): boolean {
    const token = this.token();
    const user = this.user();

    if (!token || !user) return false;

    const expiresAt = new Date(user.expiresAt).getTime();
    if (Number.isNaN(expiresAt) || expiresAt <= Date.now()) {
      this.logout();
      return false;
    }

    return true;
  }

  isAdmin(): boolean {
    return this.user()?.role === 'Admin';
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
  }

  private saveSession(response: AuthResponse): void {
    localStorage.setItem(this.tokenKey, response.token);
    localStorage.setItem(this.userKey, JSON.stringify(response));
  }
}
