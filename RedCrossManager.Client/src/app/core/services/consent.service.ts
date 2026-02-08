import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Observable } from 'rxjs';

export interface ConsentVolunteerInfo {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  email: string;
}

export interface ConsentRequestDto {
  id: string;
  volunteerId: string;
  guardianEmail?: string;
  volunteer?: ConsentVolunteerInfo;
}

export interface SubmitConsentDto {
  guardianInfo: {
    fullName: string;
    email: string;
    phone: string;
    relationship: string;
  };
  guardianAgreement: boolean;
  dataProcessingAgreement: boolean;
  signature: string;
}

@Injectable({ providedIn: 'root' })
export class ConsentService {
  constructor(private api: ApiService) {}

  getConsentRequest(volunteerId: string): Observable<ConsentRequestDto> {
    return this.api.get<ConsentRequestDto>(`/consents/${volunteerId}`);
  }

  submitConsent(volunteerId: string, payload: SubmitConsentDto): Observable<ConsentRequestDto> {
    return this.api.patch<ConsentRequestDto>(`/consents/${volunteerId}`, payload);
  }
}
