import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { LoginResponseDto } from '../models/volunteer.model';

export interface LoginRequest {
  email: string;
  password: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly isAuthenticatedSubject = new BehaviorSubject<boolean>(this.hasValidToken());

  public readonly isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router,
  ) {}

  private hasValidToken(): boolean {
    return !!localStorage.getItem('authToken');
  }

  login(email: string, password: string): Observable<LoginResponseDto> {
    const request: LoginRequest = { email, password };
    return this.http.post<LoginResponseDto>(`${this.apiUrl}/login`, request).pipe(
      tap((response) => {
        localStorage.setItem('authToken', response.accessToken);
        localStorage.setItem('userId', response.userId);
        localStorage.setItem('roles', JSON.stringify(response.roles ?? []));
        this.isAuthenticatedSubject.next(true);
      }),
    );
  }

  logout(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('userId');
    localStorage.removeItem('roles');
    this.isAuthenticatedSubject.next(false);
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    return this.hasValidToken();
  }

  getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  getUserId(): string | null {
    return localStorage.getItem('userId');
  }

  getRoles(): string[] {
    const roles = localStorage.getItem('roles');
    if (!roles) {
      return [];
    }

    try {
      const parsed = JSON.parse(roles) as string[];
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  }

  hasRole(role: string): boolean {
    return this.getRoles().includes(role);
  }
}
