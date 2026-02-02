import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DashboardService } from '../../core/services/dashboard.service';
import { VolunteerDashboardDto } from '../../core/models/dashboard.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatProgressSpinnerModule, TranslateModule, RouterLink],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit, OnDestroy {
  dashboard: VolunteerDashboardDto | null = null;
  isLoading = false;
  loadError = false;
  private destroy$ = new Subject<void>();

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.fetchDashboard();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private fetchDashboard(): void {
    this.isLoading = true;
    this.loadError = false;

    this.dashboardService.getDashboard()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (dashboard) => {
          this.dashboard = dashboard;
          this.isLoading = false;
        },
        error: () => {
          this.loadError = true;
          this.isLoading = false;
        }
      });
  }
}
