import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AdminOnboardingStep, AdminVolunteerListItem, ReviewStepRequest } from '../models/admin-dashboard.model';

@Injectable({
  providedIn: 'root'
})
export class AdminDashboardService {
  private readonly volunteersUrl = `${environment.apiUrl}/volunteers`;
  private readonly onboardingUrl = `${environment.apiUrl}/onboarding`;

  constructor(private http: HttpClient) {}

  getVolunteers(): Observable<AdminVolunteerListItem[]> {
    return this.http.get<AdminVolunteerListItem[]>(this.volunteersUrl);
  }

  getPendingOnboardingSteps(): Observable<AdminOnboardingStep[]> {
    return this.http.get<AdminOnboardingStep[]>(`${this.onboardingUrl}/steps/pending`);
  }

  reviewStep(stepId: string, request: ReviewStepRequest): Observable<void> {
    return this.http.post<void>(`${this.onboardingUrl}/steps/${stepId}/review`, request);
  }
}
