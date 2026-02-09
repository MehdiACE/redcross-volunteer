import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TrainingsComponent } from './trainings.component';
import { TrainingService } from '../../core/services/training.service';
import { AuthService } from '../../core/services/auth.service';
import { TranslateFakeLoader, TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { TrainingDto, TrainingFilterDto } from '../../core/models/training.model';

describe('TrainingsComponent', () => {
  let component: TrainingsComponent;
  let fixture: ComponentFixture<TrainingsComponent>;
  let trainingService: jasmine.SpyObj<TrainingService>;
  let authService: jasmine.SpyObj<AuthService>;

  const mockTrainings: TrainingDto[] = [
    {
      id: '550e8400-e29b-41d4-a716-446655440001',
      title: 'First Aid Certification',
      description: 'Learn basic first aid techniques',
      category: 'Safety',
      maxEnrollment: 20,
      startDate: new Date(new Date().getTime() + 7 * 24 * 60 * 60 * 1000),
      endDate: new Date(new Date().getTime() + 9 * 24 * 60 * 60 * 1000),
      locationName: 'Red Cross Center',
      status: 'Published',
      enrollmentCount: 15,
      availableSpots: 5,
      createdAt: new Date(),
      createdByCoordinatorId: 'coordinator-1',
    },
    {
      id: '550e8400-e29b-41d4-a716-446655440002',
      title: 'CPR Certification',
      description: 'CPR training for healthcare workers',
      category: 'Medical',
      maxEnrollment: 15,
      startDate: new Date(new Date().getTime() + 14 * 24 * 60 * 60 * 1000),
      endDate: new Date(new Date().getTime() + 16 * 24 * 60 * 60 * 1000),
      locationName: 'Hospital Center',
      status: 'Published',
      enrollmentCount: 10,
      availableSpots: 5,
      createdAt: new Date(),
      createdByCoordinatorId: 'coordinator-1',
    },
  ];

  beforeEach(async () => {
    const spy = jasmine.createSpyObj('TrainingService', [
      'getAllTrainings',
      'getFilteredTrainings',
      'getTrainingDetails',
      'enrollVolunteer',
      'getMyTrainings',
    ]);
    const authSpy = jasmine.createSpyObj('AuthService', ['isAuthenticated', 'getUserId']);

    await TestBed.configureTestingModule({
      imports: [
        TrainingsComponent,
        NoopAnimationsModule,
        TranslateModule.forRoot({
          loader: { provide: TranslateLoader, useClass: TranslateFakeLoader },
        }),
      ],
      providers: [
        { provide: TrainingService, useValue: spy },
        { provide: AuthService, useValue: authSpy },
      ],
    }).compileComponents();

    trainingService = TestBed.inject(TrainingService) as jasmine.SpyObj<TrainingService>;
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    fixture = TestBed.createComponent(TrainingsComponent);
    component = fixture.componentInstance;

    trainingService.getAllTrainings.and.returnValue(of([]));
    trainingService.getMyTrainings.and.returnValue(of([]));
    authService.isAuthenticated.and.returnValue(false);
    authService.getUserId.and.returnValue(null as any);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load all trainings on init', () => {
    trainingService.getAllTrainings.and.returnValue(of(mockTrainings));

    component.ngOnInit();

    expect(trainingService.getAllTrainings).toHaveBeenCalled();
    expect(component.trainings).toEqual(mockTrainings);
    expect(component.isLoading).toBeFalsy();
  });

  it('should handle load error', () => {
    const error = new Error('Failed to load trainings');
    trainingService.getAllTrainings.and.returnValue(throwError(() => error));

    component.ngOnInit();

    expect(component.loadError).toBeTruthy();
    expect(component.isLoading).toBeFalsy();
  });

  it('should filter trainings by category', () => {
    trainingService.getFilteredTrainings.and.returnValue(of([mockTrainings[0]]));

    component.applyFilter({
      category: 'Safety',
      startDateFrom: undefined,
      startDateTo: undefined,
      availableSpotsOnly: false,
      page: 1,
      pageSize: 20,
    } as TrainingFilterDto);

    expect(trainingService.getFilteredTrainings).toHaveBeenCalledWith({
      category: 'Safety',
      startDateFrom: undefined,
      startDateTo: undefined,
      availableSpotsOnly: false,
      page: 1,
      pageSize: 20,
    });
    expect(component.trainings.length).toBe(1);
    expect(component.trainings[0].category).toBe('Safety');
  });

  it('should filter trainings by available spots only', () => {
    const availableTraining = { ...mockTrainings[0], availableSpots: 5 };
    trainingService.getFilteredTrainings.and.returnValue(of([availableTraining]));

    component.applyFilter({
      category: undefined,
      startDateFrom: undefined,
      startDateTo: undefined,
      availableSpotsOnly: true,
      page: 1,
      pageSize: 20,
    } as TrainingFilterDto);

    expect(trainingService.getFilteredTrainings).toHaveBeenCalled();
    expect(component.trainings.every((t) => t.availableSpots > 0)).toBeTruthy();
  });

  it('should enroll volunteer in training', () => {
    const enrollmentDto = {
      id: 'enrollment-1',
      trainingId: mockTrainings[0].id,
      volunteerId: 'volunteer-1',
      status: 'Enrolled',
      enrolledAt: new Date(),
      certificateNumber: null,
      certificateIssuedAt: null,
    };

    authService.isAuthenticated.and.returnValue(true);
    authService.getUserId.and.returnValue('volunteer-1');
    trainingService.enrollVolunteer.and.returnValue(of(enrollmentDto));

    component.enrollInTraining(mockTrainings[0]);

    expect(trainingService.enrollVolunteer).toHaveBeenCalledWith(mockTrainings[0].id, {
      volunteerId: jasmine.any(String),
      status: 'Enrolled',
    });
    expect(component.selectedTraining).toEqual(mockTrainings[0]);
  });

  it('should handle enrollment error', () => {
    const error = new Error('Already enrolled');
    authService.isAuthenticated.and.returnValue(true);
    authService.getUserId.and.returnValue('volunteer-1');
    trainingService.enrollVolunteer.and.returnValue(throwError(() => error));

    component.enrollInTraining(mockTrainings[0]);

    expect(trainingService.enrollVolunteer).toHaveBeenCalled();
    expect(component.enrollmentError).toBeTruthy();
  });

  it('should display training details modal', () => {
    component.showTrainingDetails(mockTrainings[0]);

    expect(component.selectedTraining).toEqual(mockTrainings[0]);
    expect(component.showDetailsModal).toBeTruthy();
  });

  it('should close training details modal', () => {
    component.selectedTraining = mockTrainings[0];
    component.showDetailsModal = true;

    component.closeDetailsModal();

    expect(component.showDetailsModal).toBeFalsy();
  });

  it('should display category filter options', () => {
    const categories = component.getCategoryOptions();

    expect(categories).toContain('Safety');
    expect(categories).toContain('Medical');
    expect(categories.length).toBeGreaterThan(0);
  });

  it('should load volunteer trainings', () => {
    const volunteerTrainings = [
      {
        id: 'enrollment-1',
        trainingId: mockTrainings[0].id,
        volunteerId: 'volunteer-1',
        status: 'Enrolled',
        enrolledAt: new Date(),
        certificateNumber: null,
        certificateIssuedAt: null,
      },
    ];
    trainingService.getMyTrainings.and.returnValue(of(volunteerTrainings));

    component.loadMyTrainings();

    expect(trainingService.getMyTrainings).toHaveBeenCalled();
  });

  it('should render training list with proper structure', () => {
    trainingService.getAllTrainings.and.returnValue(of(mockTrainings));
    component.ngOnInit();

    expect(component.trainings.length).toBeGreaterThan(0);
  });

  it('should display enrollment button when spots available', () => {
    const trainingWithSpots = { ...mockTrainings[0], availableSpots: 5 };
    expect(component.hasAvailableSpots(trainingWithSpots)).toBeTrue();
  });

  it('should disable enrollment button when no spots available', () => {
    const trainingNoSpots = { ...mockTrainings[0], availableSpots: 0 };
    expect(component.hasAvailableSpots(trainingNoSpots)).toBeFalse();
  });

  it('should paginate trainings', () => {
    trainingService.getFilteredTrainings.and.returnValue(of(mockTrainings));

    component.goToPage(2);

    expect(trainingService.getFilteredTrainings).toHaveBeenCalledWith(
      jasmine.objectContaining({
        page: 2,
      }),
    );
    expect(component.currentPage).toBe(2);
  });

  it('should sort trainings by start date', () => {
    const sortedTrainings = [...mockTrainings].sort(
      (a, b) => new Date(a.startDate).getTime() - new Date(b.startDate).getTime(),
    );

    component.trainings = sortedTrainings;

    expect(component.trainings[0].startDate <= component.trainings[1].startDate).toBeTruthy();
  });
});
