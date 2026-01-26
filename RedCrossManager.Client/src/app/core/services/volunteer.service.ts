import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RegisterVolunteerDto, VolunteerDto, SmsOptInDto } from '../models/volunteer.model';

@Injectable({
  providedIn: 'root'
})
export class VolunteerService {
  private readonly apiUrl = `${environment.apiUrl}/volunteers`;

  constructor(private http: HttpClient) {}

  register(dto: RegisterVolunteerDto): Observable<VolunteerDto> {
    return this.http.post<VolunteerDto>(`${this.apiUrl}/register`, dto);
  }

  getById(id: string): Observable<VolunteerDto> {
    return this.http.get<VolunteerDto>(`${this.apiUrl}/${id}`);
  }

  getByEmail(email: string): Observable<VolunteerDto> {
    return this.http.get<VolunteerDto>(`${this.apiUrl}/by-email/${encodeURIComponent(email)}`);
  }

  getProfile(): Observable<VolunteerDto> {
    return this.http.get<VolunteerDto>(`${this.apiUrl}/me`);
  }

  updateSmsOptIn(smsOptIn: boolean): Observable<VolunteerDto> {
    const dto: SmsOptInDto = { smsOptIn };
    return this.http.post<VolunteerDto>(`${this.apiUrl}/me/sms-opt-in`, dto);
  }
}

