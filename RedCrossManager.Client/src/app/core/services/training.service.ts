import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  TrainingDto,
  TrainingDetailDto,
  TrainingFilterDto,
  EnrollTrainingDto,
  TrainingEnrollmentDto
} from '../models/training.model';

@Injectable({
  providedIn: 'root'
})
export class TrainingService {
  private apiUrl = `${environment.apiUrl}/trainings`;

  constructor(private http: HttpClient) {}

  getAllTrainings(): Observable<TrainingDto[]> {
    return this.http.get<TrainingDto[]>(this.apiUrl);
  }

  getFilteredTrainings(filter: TrainingFilterDto): Observable<TrainingDto[]> {
    return this.http.get<TrainingDto[]>(`${this.apiUrl}/filtered`, {
      params: {
        ...(filter.category && { category: filter.category }),
        ...(filter.startDateFrom && { startDateFrom: filter.startDateFrom.toISOString() }),
        ...(filter.startDateTo && { startDateTo: filter.startDateTo.toISOString() }),
        ...(filter.availableSpotsOnly !== undefined && {
          availableSpotsOnly: filter.availableSpotsOnly.toString()
        }),
        page: filter.page.toString(),
        pageSize: filter.pageSize.toString()
      }
    });
  }

  getTrainingDetails(trainingId: string): Observable<TrainingDetailDto> {
    return this.http.get<TrainingDetailDto>(`${this.apiUrl}/${trainingId}`);
  }

  enrollVolunteer(trainingId: string, dto: EnrollTrainingDto): Observable<TrainingEnrollmentDto> {
    return this.http.post<TrainingEnrollmentDto>(
      `${this.apiUrl}/${trainingId}/enroll`,
      dto
    );
  }

  getTrainingEnrollments(trainingId: string): Observable<TrainingEnrollmentDto[]> {
    return this.http.get<TrainingEnrollmentDto[]>(
      `${this.apiUrl}/${trainingId}/enrollments`
    );
  }

  getMyTrainings(): Observable<TrainingEnrollmentDto[]> {
    return this.http.get<TrainingEnrollmentDto[]>(`${this.apiUrl}/volunteer/my-trainings`);
  }
}
