import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
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
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TranslateModule } from '@ngx-translate/core';
import { TranslateService } from '@ngx-translate/core';
import { MissionService } from '../../core/services/mission.service';
import { AuthService } from '../../core/services/auth.service';
import { MissionDto } from '../../core/models/mission.model';

@Component({
  selector: 'app-missions',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
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
    MatSnackBarModule,
    TranslateModule
  ],
  templateUrl: './missions.component.html',
  styleUrls: ['./missions.component.scss']
})
export class MissionsComponent implements OnInit {
  missions: MissionDto[] = [];
  filteredMissions: MissionDto[] = [];
  isLoading = true;
  loadError = false;
  selectedMission: MissionDto | null = null;
  showDetailsModal = false;

  typeFilter = '';
  startDateFilter: Date | null = null;
  endDateFilter: Date | null = null;
  availableSpotsOnly = false;

  constructor(
    private missionService: MissionService,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadMissions();
  }

  loadMissions(): void {
    this.isLoading = true;
    this.loadError = false;

    this.missionService.getMissions().subscribe({
      next: (missions) => {
        this.missions = missions ?? [];
        this.applyFilters();
        this.isLoading = false;
      },
      error: () => {
        this.loadError = true;
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    this.filteredMissions = this.missions.filter((mission) => {
      const missionStart = this.toDate(mission.startAt);
      const missionEnd = this.toDate(mission.endAt);

      if (this.typeFilter && mission.missionType !== this.typeFilter) {
        return false;
      }

      if (this.startDateFilter && missionStart < this.startDateFilter) {
        return false;
      }

      if (this.endDateFilter && missionEnd > this.endDateFilter) {
        return false;
      }

      if (this.availableSpotsOnly && mission.availableSlots <= 0) {
        return false;
      }

      return true;
    });
  }

  clearFilters(): void {
    this.typeFilter = '';
    this.startDateFilter = null;
    this.endDateFilter = null;
    this.availableSpotsOnly = false;
    this.applyFilters();
  }

  showMissionDetails(mission: MissionDto): void {
    this.selectedMission = mission;
    this.showDetailsModal = true;
  }

  closeDetailsModal(): void {
    this.showDetailsModal = false;
    this.selectedMission = null;
  }

  applyToMission(mission: MissionDto): void {
    if (!this.authService.isAuthenticated()) {
      this.snackBar.open(this.translate.instant('missions.authRequired'), undefined, { duration: 3000 });
      return;
    }

    const volunteerId = this.authService.getUserId();
    if (!volunteerId) {
      this.snackBar.open(this.translate.instant('missions.missingVolunteerId'), undefined, { duration: 3000 });
      return;
    }

    this.missionService.applyToMission(mission.id, { volunteerId }).subscribe({
      next: () => {
        this.snackBar.open(this.translate.instant('missions.applySuccess'), undefined, { duration: 3000 });
        this.loadMissions();
      },
      error: () => {
        this.snackBar.open(this.translate.instant('missions.applyError'), undefined, { duration: 3000 });
      }
    });
  }

  getMissionTypes(): string[] {
    return Array.from(new Set(this.missions.map((m) => m.missionType))).sort();
  }

  hasAvailableSlots(mission: MissionDto): boolean {
    return mission.availableSlots > 0;
  }

  getAvailabilityPercent(mission: MissionDto): number {
    if (!mission.volunteersNeeded || mission.volunteersNeeded <= 0) {
      return 0;
    }

    const ratio = (mission.availableSlots / mission.volunteersNeeded) * 100;
    return Math.max(0, Math.min(100, Math.round(ratio)));
  }

  private toDate(value: Date | string): Date {
    return value instanceof Date ? value : new Date(value);
  }
}
