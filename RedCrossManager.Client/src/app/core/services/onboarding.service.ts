import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { OnboardingProgressDto, SubmitStepDto } from '../models/onboarding.model';

@Injectable({
  providedIn: 'root',
})
export class OnboardingService {
  private readonly apiUrl = `${environment.apiUrl}/onboarding`;

  constructor(private http: HttpClient) {}

  getMyProgress(): Observable<OnboardingProgressDto> {
    return this.http.get<OnboardingProgressDto>(`${this.apiUrl}/me`);
  }

  submitMyStep(stepId: string): Observable<any> {
    const dto: SubmitStepDto = { stepId };
    return this.http.post(`${this.apiUrl}/me/steps/submit`, dto);
  }

  getProgress(volunteerId: string): Observable<OnboardingProgressDto> {
    return this.http.get<OnboardingProgressDto>(`${this.apiUrl}/progress/${volunteerId}`);
  }

  submitStep(volunteerId: string, stepId: string): Observable<any> {
    const dto: SubmitStepDto = { stepId };
    return this.http.post(`${this.apiUrl}/${volunteerId}/steps/submit`, dto);
  }

  resumeProgress(volunteerId: string): Observable<OnboardingProgressDto> {
    return this.http.get<OnboardingProgressDto>(`${this.apiUrl}/progress/${volunteerId}`);
  }
}
