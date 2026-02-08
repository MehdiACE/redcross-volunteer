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
  useMockData = true;

  typeFilter = '';
  startDateFilter: Date | null = null;
  endDateFilter: Date | null = null;
  availableSpotsOnly = false;

  private readonly mockMissions: Array<MissionDto & { urgent?: boolean; icon?: string; iconClass?: string }> = [
    this.createMockMission({
      id: 'mock-1',
      title: 'Secouriste de Proximité',
      missionType: 'Urgence',
      description: 'Intervention de proximité lors d’événements sportifs.',
      startAt: new Date('2026-02-12T08:00:00'),
      endAt: new Date('2026-02-12T18:00:00'),
      location: 'Stade de France, Saint-Denis',
      availableSlots: 3,
      volunteersNeeded: 5,
      urgent: true,
      icon: 'medical_services',
      iconClass: 'bg-blue-50 text-blue-600'
    }),
    this.createMockMission({
      id: 'mock-2',
      title: 'Distribution de Repas',
      missionType: 'Solidarité',
      description: 'Distribution de repas aux personnes vulnérables.',
      startAt: new Date('2026-02-13T19:00:00'),
      endAt: new Date('2026-02-13T22:00:00'),
      location: 'Centre Social, Paris 11e',
      availableSlots: 1,
      volunteersNeeded: 8,
      icon: 'restaurant',
      iconClass: 'bg-amber-50 text-amber-600'
    }),
    this.createMockMission({
      id: 'mock-3',
      title: 'Soutien Scolaire',
      missionType: 'Éducation',
      description: 'Accompagnement scolaire pour collégiens.',
      startAt: new Date('2026-02-14T14:00:00'),
      endAt: new Date('2026-02-14T17:00:00'),
      location: 'Antenne Locale, Lyon 3e',
      availableSlots: 4,
      volunteersNeeded: 4,
      icon: 'menu_book',
      iconClass: 'bg-purple-50 text-purple-600'
    }),
    this.createMockMission({
      id: 'mock-4',
      title: 'Écoute Psychologique',
      missionType: 'Soutien',
      description: 'Permanence d’écoute et d’orientation.',
      startAt: new Date('2026-02-16T18:00:00'),
      endAt: new Date('2026-02-16T21:00:00'),
      location: 'Télétravail / Antenne',
      availableSlots: 2,
      volunteersNeeded: 3,
      icon: 'psychology',
      iconClass: 'bg-emerald-50 text-emerald-600'
    }),
    this.createMockMission({
      id: 'mock-5',
      title: 'Chauffeur Logistique',
      missionType: 'Logistique',
      description: 'Transport de matériel et de denrées.',
      startAt: new Date('2026-02-17T09:00:00'),
      endAt: new Date('2026-02-17T17:00:00'),
      location: 'Entrepôt Central, Bordeaux',
      availableSlots: 5,
      volunteersNeeded: 6,
      icon: 'local_shipping',
      iconClass: 'bg-pink-50 text-pink-600'
    }),
    this.createMockMission({
      id: 'mock-6',
      title: 'Visites aux Seniors',
      missionType: 'Social',
      description: 'Visites et activités avec des seniors.',
      startAt: new Date('2026-02-18T15:00:00'),
      endAt: new Date('2026-02-18T18:00:00'),
      location: 'EHPAD Belle Vue, Marseille',
      availableSlots: 0,
      volunteersNeeded: 4,
      icon: 'groups',
      iconClass: 'bg-gray-100 text-gray-500'
    })
  ];

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

    if (this.useMockData) {
      this.missions = this.mockMissions as MissionDto[];
      this.applyFilters();
      this.isLoading = false;
      return;
    }

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

  isUrgent(mission: MissionDto): boolean {
    return Boolean((mission as MissionDto & { urgent?: boolean }).urgent);
  }

  getMissionIcon(mission: MissionDto): string {
    return (mission as MissionDto & { icon?: string }).icon || 'medical_services';
  }

  getMissionIconClass(mission: MissionDto): string {
    return (mission as MissionDto & { iconClass?: string }).iconClass || 'bg-red-50 text-red-600';
  }

  private createMockMission(
    data: Omit<
      MissionDto,
      'requiredCertifications' | 'travelBufferMinutes' | 'published' | 'createdAt' | 'createdBy'
    > & {
      urgent?: boolean;
      icon?: string;
      iconClass?: string;
    }
  ): MissionDto & { urgent?: boolean; icon?: string; iconClass?: string } {
    return {
      requiredCertifications: [],
      travelBufferMinutes: 0,
      published: true,
      createdAt: new Date(),
      createdBy: 'seed',
      ...data
    };
  }

  getMissionDateRange(mission: MissionDto): string {
    const start = this.toDate(mission.startAt);
    const end = this.toDate(mission.endAt);
    const dateFormatter = new Intl.DateTimeFormat('fr-FR', {
      weekday: 'short',
      day: '2-digit',
      month: 'long'
    });
    const timeFormatter = new Intl.DateTimeFormat('fr-FR', {
      hour: '2-digit',
      minute: '2-digit'
    });

    return `${dateFormatter.format(start)} - ${timeFormatter.format(start)} à ${timeFormatter.format(end)}`;
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
