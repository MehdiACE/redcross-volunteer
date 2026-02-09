import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { TranslateFakeLoader, TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { of } from 'rxjs';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { StepperComponent } from './stepper.component';
import { OnboardingService } from '../../../core/services/onboarding.service';
import { AuthService } from '../../../core/services/auth.service';

describe('StepperComponent', () => {
  let component: StepperComponent;
  let fixture: ComponentFixture<StepperComponent>;
  let onboardingService: jasmine.SpyObj<OnboardingService>;

  const mockProgress = {
    volunteerId: '123',
    volunteer: {
      firstName: 'Jane',
      lastName: 'Doe',
      email: 'jane.doe@example.com',
      phone: '+15145551234',
    },
    steps: [
      {
        id: 'step-1',
        stepNumber: 1,
        title: 'Profile',
        description: 'Complete profile',
        status: 'Completed',
      },
      {
        id: 'step-2',
        stepNumber: 2,
        title: 'Orientation',
        description: 'Orientation',
        status: 'Pending',
      },
      {
        id: 'step-3',
        stepNumber: 3,
        title: 'Training',
        description: 'Training',
        status: 'Pending',
      },
      {
        id: 'step-4',
        stepNumber: 4,
        title: 'Final Review',
        description: 'Final review',
        status: 'Pending',
      },
    ],
    currentStatus: 'Pending',
    isMinor: false,
    parentalConsentApproved: false,
    startedAt: new Date(),
  };

  beforeEach(async () => {
    const onboardingServiceSpy = jasmine.createSpyObj('OnboardingService', [
      'getMyProgress',
      'submitMyStep',
    ]);
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['hasRole']);
    await TestBed.configureTestingModule({
      imports: [
        StepperComponent,
        MatSnackBarModule,
        RouterTestingModule,
        TranslateModule.forRoot({
          loader: { provide: TranslateLoader, useClass: TranslateFakeLoader },
        }),
      ],
      providers: [
        { provide: OnboardingService, useValue: onboardingServiceSpy },
        { provide: AuthService, useValue: authServiceSpy },
      ],
    }).compileComponents();

    onboardingService = TestBed.inject(OnboardingService) as jasmine.SpyObj<OnboardingService>;
    onboardingService.getMyProgress.and.returnValue(of(mockProgress as any));
    onboardingService.submitMyStep.and.returnValue(of({} as any));

    const authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    authService.hasRole.and.returnValue(false);

    const router = TestBed.inject(Router);
    spyOn(router, 'navigate');

    fixture = TestBed.createComponent(StepperComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load progress and set selected step index', () => {
    fixture.detectChanges();

    expect(onboardingService.getMyProgress).toHaveBeenCalled();
    expect(component.progress).toBeTruthy();
    expect(component.selectedStepIndex).toBe(1);
  });

  it('should lock steps for minors without parental consent', () => {
    onboardingService.getMyProgress.and.returnValue(
      of({ ...mockProgress, isMinor: true, parentalConsentApproved: false } as any),
    );

    fixture.detectChanges();

    expect(component.isStepLocked(1)).toBe(true);
  });

  it('should calculate progress percentage', () => {
    fixture.detectChanges();

    expect(component.getProgressPercentage()).toBe(25);
  });

  it('should submit step using submitMyStep', () => {
    fixture.detectChanges();

    component.submitStep(0);

    expect(onboardingService.submitMyStep).toHaveBeenCalledWith('step-1');
  });
});
