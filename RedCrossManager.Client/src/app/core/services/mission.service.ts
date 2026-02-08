import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  MissionDto,
  ApplyMissionDto,
  AssignVolunteersDto,
  AssignmentDto
} from '../models/mission.model';

@Injectable({
  providedIn: 'root'
})
export class MissionService {
  private apiUrl = `${environment.apiUrl}/missions`;

  constructor(private http: HttpClient) {}

  getMissions(): Observable<MissionDto[]> {
    return this.http.get<MissionDto[]>(this.apiUrl);
  }

  getMission(id: string): Observable<MissionDto> {
    return this.http.get<MissionDto>(`${this.apiUrl}/${id}`);
  }

  applyToMission(missionId: string, dto: ApplyMissionDto): Observable<AssignmentDto> {
    return this.http.post<AssignmentDto>(`${this.apiUrl}/${missionId}/apply`, dto);
  }

  assignVolunteers(missionId: string, dto: AssignVolunteersDto): Observable<AssignmentDto[]> {
    return this.http.post<AssignmentDto[]>(`${this.apiUrl}/${missionId}/assign`, dto);
  }

  confirmAssignment(assignmentId: string): Observable<AssignmentDto> {
    return this.http.post<AssignmentDto>(`${environment.apiUrl}/assignments/${assignmentId}/confirm`, {});
  }

  updateAssignmentStatus(assignmentId: string, status: string): Observable<AssignmentDto> {
    return this.http.post<AssignmentDto>(`${environment.apiUrl}/assignments/${assignmentId}/status`, { status });
  }
}
