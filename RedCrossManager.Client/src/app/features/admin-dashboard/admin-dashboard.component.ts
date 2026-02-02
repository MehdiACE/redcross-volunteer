import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import { AgGridAngular } from 'ag-grid-angular';
import { ClientSideRowModelModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AdminDashboardService } from '../../core/services/admin-dashboard.service';
import { AdminOnboardingStep, AdminVolunteerListItem } from '../../core/models/admin-dashboard.model';

ModuleRegistry.registerModules([ClientSideRowModelModule]);

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, MatProgressSpinnerModule, TranslateModule, AgGridAngular],
  templateUrl: './admin-dashboard.component.html'
})
export class AdminDashboardComponent implements OnInit, OnDestroy {
  volunteers: AdminVolunteerListItem[] = [];
  pendingSteps: AdminOnboardingStep[] = [];
  isLoadingVolunteers = false;
  isLoadingSteps = false;
  volunteerError = false;
  stepsError = false;

  colDefs: ColDef[] = [
    {
      headerName: 'Name',
      valueGetter: params => `${params.data?.firstName ?? ''} ${params.data?.lastName ?? ''}`.trim(),
      flex: 1,
      minWidth: 180
    },
    { field: 'email', headerName: 'Email', flex: 1, minWidth: 220 },
    { field: 'status', headerName: 'Status', minWidth: 120 },
    { field: 'registeredAt', headerName: 'Registered', minWidth: 140 },
    { field: 'languagePreference', headerName: 'Lang', minWidth: 90 },
    { field: 'isMinor', headerName: 'Minor', minWidth: 90 }
  ];

  defaultColDef: ColDef = {
    sortable: true,
    filter: true,
    resizable: true
  };

  private destroy$ = new Subject<void>();

  constructor(private adminDashboardService: AdminDashboardService) {}

  ngOnInit(): void {
    this.loadVolunteers();
    this.loadPendingSteps();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  approveStep(step: AdminOnboardingStep): void {
    this.adminDashboardService.reviewStep(step.id, { approved: true })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => this.loadPendingSteps(),
        error: () => {
          this.stepsError = true;
        }
      });
  }

  rejectStep(step: AdminOnboardingStep): void {
    this.adminDashboardService.reviewStep(step.id, { approved: false })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => this.loadPendingSteps(),
        error: () => {
          this.stepsError = true;
        }
      });
  }

  private loadVolunteers(): void {
    this.isLoadingVolunteers = true;
    this.volunteerError = false;
    this.adminDashboardService.getVolunteers()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: data => {
          this.volunteers = data;
          this.isLoadingVolunteers = false;
        },
        error: () => {
          this.volunteerError = true;
          this.isLoadingVolunteers = false;
        }
      });
  }

  private loadPendingSteps(): void {
    this.isLoadingSteps = true;
    this.stepsError = false;
    this.adminDashboardService.getPendingOnboardingSteps()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: data => {
          this.pendingSteps = data;
          this.isLoadingSteps = false;
        },
        error: () => {
          this.stepsError = true;
          this.isLoadingSteps = false;
        }
      });
  }
}
