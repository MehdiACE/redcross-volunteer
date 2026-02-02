import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { VolunteerDashboardDto } from '../models/dashboard.model';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly apiUrl = `${environment.apiUrl}/volunteers`;

  constructor(private http: HttpClient) {}

  getDashboard(): Observable<VolunteerDashboardDto> {
    return this.http.get<VolunteerDashboardDto>(`${this.apiUrl}/me`);
  }
}
