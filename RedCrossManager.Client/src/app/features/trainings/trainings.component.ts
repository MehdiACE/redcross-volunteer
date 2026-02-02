import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatTabsModule } from '@angular/material/tabs';
import { TranslateModule } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { TrainingService } from '../../core/services/training.service';
import {
  TrainingDto,
  TrainingDetailDto,
  TrainingFilterDto,
  EnrollTrainingDto
} from '../../core/models/training.model';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-trainings',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatTabsModule,
    TranslateModule
  ],
  templateUrl: './trainings.component.html',
  styleUrls: ['./trainings.component.scss']
})
export class TrainingsComponent implements OnInit, OnDestroy {
  trainings: TrainingDto[] = [];
  myTrainings: any[] = [];
  isLoading = true;
  loadError = false;
  enrollmentError = false;
  selectedTraining: TrainingDto | null = null;
  showDetailsModal = false;
  currentPage = 1;
  pageSize = 10;
  categoryFilter = '';
  startDateFilter: Date | null = null;
  endDateFilter: Date | null = null;
  availableSpotsOnlyFilter = false;
  private destroy$ = new Subject<void>();

  categories = [
    'Safety',
    'Medical',
    'Training',
    'Orientation',
    'Other'
  ];

  constructor(
    private trainingService: TrainingService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadAllTrainings();
    if (this.authService.isAuthenticated()) {
      this.loadMyTrainings();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadAllTrainings(): void {
    this.isLoading = true;
    this.loadError = false;
    this.trainingService.getAllTrainings()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.trainings = data;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Failed to load trainings:', error);
          this.loadError = true;
          this.isLoading = false;
        }
      });
  }

  loadMyTrainings(): void {
    this.trainingService.getMyTrainings()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.myTrainings = data;
        },
        error: (error) => {
          console.error('Failed to load my trainings:', error);
        }
      });
  }

  applyFilter(filter: TrainingFilterDto): void {
    this.isLoading = true;
    this.trainingService.getFilteredTrainings(filter)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.trainings = data;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Failed to filter trainings:', error);
          this.loadError = true;
          this.isLoading = false;
        }
      });
  }

  applyFilters(): void {
    const filter: TrainingFilterDto = {
      category: this.categoryFilter || undefined,
      startDateFrom: this.startDateFilter || undefined,
      startDateTo: this.endDateFilter || undefined,
      availableSpotsOnly: this.availableSpotsOnlyFilter,
      page: this.currentPage,
      pageSize: this.pageSize
    };
    this.applyFilter(filter);
  }

  clearFilters(): void {
    this.categoryFilter = '';
    this.startDateFilter = null;
    this.endDateFilter = null;
    this.availableSpotsOnlyFilter = false;
    this.currentPage = 1;
    this.loadAllTrainings();
  }

  enrollInTraining(training: TrainingDto): void {
    if (!this.authService.isAuthenticated()) {
      console.error('User not authenticated');
      return;
    }

    const userId = this.authService.getUserId();
    if (!userId) {
      console.error('User ID not found');
      return;
    }

    const enrollDto: EnrollTrainingDto = {
      volunteerId: userId,
      status: 'Enrolled'
    };

    this.trainingService.enrollVolunteer(training.id, enrollDto)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (enrollment) => {
          console.log('Enrolled successfully:', enrollment);
          this.enrollmentError = false;
          this.selectedTraining = training;
          this.loadMyTrainings();
        },
        error: (error) => {
          console.error('Failed to enroll:', error);
          this.enrollmentError = true;
        }
      });
  }

  showTrainingDetails(training: TrainingDto): void {
    this.selectedTraining = training;
    this.showDetailsModal = true;
  }

  closeDetailsModal(): void {
    this.showDetailsModal = false;
    this.selectedTraining = null;
  }

  getCategoryOptions(): string[] {
    return this.categories;
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.applyFilters();
  }

  hasAvailableSpots(training: TrainingDto): boolean {
    return training.availableSpots > 0;
  }

  isAlreadyEnrolled(training: TrainingDto): boolean {
    return this.myTrainings.some(t => t.trainingId === training.id);
  }

  getEnrollmentStatus(training: TrainingDto): string {
    const enrollment = this.myTrainings.find(t => t.trainingId === training.id);
    return enrollment?.status || '';
  }
}
