import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { TranslateModule } from '@ngx-translate/core';
import { DashboardComponent } from './dashboard.component';
import { DashboardService } from '../../core/services/dashboard.service';
import { VolunteerDashboardDto } from '../../core/models/dashboard.model';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let dashboardService: jasmine.SpyObj<DashboardService>;

  const mockDashboard: VolunteerDashboardDto = {
    id: 'vol-1',
    firstName: 'Amina',
    lastName: 'Diallo',
    email: 'amina@example.com',
    phone: '+15145551234',
    dateOfBirth: new Date('1990-01-01'),
    status: 'Pending',
    languagePreference: 'fr',
    registeredAt: new Date(),
    isMinor: false,
    smsOptIn: false,
    onboarding: {
      completedCount: 1,
      totalCount: 4,
      currentStepNumber: 2,
      currentStep: 'OrientationTraining',
      isComplete: false,
      isMinor: false,
      parentalConsentApproved: false
    },
    upcomingAssignments: [
      {
        id: 'assign-1',
        title: 'Blood Drive',
        startAt: new Date().toISOString(),
        endAt: new Date().toISOString(),
        location: 'Montreal',
        status: 'Confirmed'
      }
    ],
    trainings: [],
    certifications: [],
    alerts: [
      {
        type: 'Certification Expiring',
        message: 'Your First Aid certification expires soon.'
      }
    ]
  };

  beforeEach(async () => {
    const dashboardServiceSpy = jasmine.createSpyObj('DashboardService', ['getDashboard']);

    await TestBed.configureTestingModule({
      imports: [DashboardComponent, TranslateModule.forRoot()],
      providers: [{ provide: DashboardService, useValue: dashboardServiceSpy }]
    }).compileComponents();

    dashboardService = TestBed.inject(DashboardService) as jasmine.SpyObj<DashboardService>;
    dashboardService.getDashboard.and.returnValue(of(mockDashboard));

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load dashboard data on init', (done) => {
    fixture.detectChanges();

    setTimeout(() => {
      expect(dashboardService.getDashboard).toHaveBeenCalled();
      expect(component.dashboard).toBeTruthy();
      done();
    }, 100);
  });

  it('should render dashboard cards and alerts', (done) => {
    fixture.detectChanges();

    setTimeout(() => {
      const element = fixture.nativeElement as HTMLElement;
      const statusCard = element.querySelector('[data-testid="dashboard-status-card"]');
      const alerts = element.querySelectorAll('[data-testid="dashboard-alert-item"]');
      const assignments = element.querySelector('[data-testid="dashboard-assignments"]');

      expect(statusCard).toBeTruthy();
      expect(assignments).toBeTruthy();
      expect(alerts.length).toBe(1);
      done();
    }, 100);
  });

  it('should handle dashboard load errors', (done) => {
    dashboardService.getDashboard.and.returnValue(
      throwError(() => new Error('Load error'))
    );

    component.ngOnInit();

    setTimeout(() => {
      expect(component.isLoading).toBe(false);
      expect(component.loadError).toBe(true);
      done();
    }, 100);
  });
});
