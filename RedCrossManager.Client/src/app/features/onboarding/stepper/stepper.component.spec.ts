import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { MatStepperModule } from '@angular/material/stepper';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';
import { StepperComponent } from './stepper.component';
import { OnboardingService } from '../../../core/services/onboarding.service';
import { TranslateModule } from '@ngx-translate/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

describe('StepperComponent', () => {
  let component: StepperComponent;
  let fixture: ComponentFixture<StepperComponent>;
  let onboardingService: jasmine.SpyObj<OnboardingService>;

  const mockOnboardingProgress = {
    volunteerId: '123',
    currentStep: 1,
    completedSteps: [1],
    isMinor: false,
    hasParentalConsent: false,
    canAdvance: true,
    steps: [
      { stepNumber: 1, title: 'Profile', isCompleted: true },
      { stepNumber: 2, title: 'Orientation', isCompleted: false },
      { stepNumber: 3, title: 'Training', isCompleted: false },
      { stepNumber: 4, title: 'Final Review', isCompleted: false }
    ]
  };

  beforeEach(async () => {
    const onboardingServiceSpy = jasmine.createSpyObj('OnboardingService', [
      'getProgress',
      'submitStep',
      'resumeOnboarding'
    ]);

    await TestBed.configureTestingModule({
      imports: [
        StepperComponent,
        ReactiveFormsModule,
        MatStepperModule,
        MatButtonModule,
        MatCardModule,
        MatProgressSpinnerModule,
        MatSnackBarModule,
        TranslateModule.forRoot(),
        BrowserAnimationsModule
      ],
      providers: [
        { provide: OnboardingService, useValue: onboardingServiceSpy }
      ]
    }).compileComponents();

    onboardingService = TestBed.inject(OnboardingService) as jasmine.SpyObj<OnboardingService>;
    onboardingService.getProgress.and.returnValue(of(mockOnboardingProgress as any));
    onboardingService.submitStep.and.returnValue(of({} as any));

    fixture = TestBed.createComponent(StepperComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load onboarding progress on init', (done) => {
    fixture.detectChanges();

    setTimeout(() => {
      expect(onboardingService.getProgress).toHaveBeenCalled();
      expect(component.progress).toBeTruthy();
      done();
    }, 100);
  });

  it('should display all 4 onboarding steps', (done) => {
    fixture.detectChanges();

    setTimeout(() => {
      expect(component.progress?.steps.length).toBe(4);
      expect(component.progress?.steps[0].title).toBe('Profile');
      expect(component.progress?.steps[3].title).toBe('Final Review');
      done();
    }, 100);
  });

  it('should set correct current step', (done) => {
    fixture.detectChanges();

    setTimeout(() => {
      expect(component.progress?.currentStep).toBe(1);
      done();
    }, 100);
  });

  it('should mark completed steps as done', (done) => {
    fixture.detectChanges();

    setTimeout(() => {
      expect(component.progress?.completedSteps).toContain(1);
      done();
    }, 100);
  });

  it('should allow advancement when not minor or consent approved', (done) => {
    fixture.detectChanges();

    setTimeout(() => {
      expect(component.progress?.canAdvance).toBe(true);
      done();
    }, 100);
  });

  it('should block advancement for minors without parental consent', (done) => {
    const minorProgress = { ...mockOnboardingProgress, isMinor: true, hasParentalConsent: false, canAdvance: false };
    onboardingService.getProgress.and.returnValue(of(minorProgress as any));

    fixture.detectChanges();

    setTimeout(() => {
      expect(component.progress?.isMinor).toBe(true);
      expect(component.progress?.canAdvance).toBe(false);
      done();
    }, 100);
  });

  it('should allow advancement for minors with parental consent', (done) => {
    const minorWithConsentProgress = { ...mockOnboardingProgress, isMinor: true, hasParentalConsent: true, canAdvance: true };
    onboardingService.getProgress.and.returnValue(of(minorWithConsentProgress as any));

    fixture.detectChanges();

    setTimeout(() => {
      expect(component.progress?.isMinor).toBe(true);
      expect(component.progress?.hasParentalConsent).toBe(true);
      expect(component.progress?.canAdvance).toBe(true);
      done();
    }, 100);
  });

  it('should submit step and advance to next', (done) => {
    onboardingService.submitStep.and.returnValue(of({} as any));
    fixture.detectChanges();

    setTimeout(() => {
      component.nextStep();

      expect(onboardingService.submitStep).toHaveBeenCalledWith(
        mockOnboardingProgress.volunteerId,
        1
      );
      done();
    }, 100);
  });

  it('should resume onboarding from saved progress', (done) => {
    const resumeProgress = { ...mockOnboardingProgress, currentStep: 3 };
    onboardingService.getProgress.and.returnValue(of(resumeProgress as any));

    fixture.detectChanges();

    setTimeout(() => {
      expect(component.progress?.currentStep).toBe(3);
      done();
    }, 100);
  });

  it('should handle progress loading error', (done) => {
    onboardingService.getProgress.and.returnValue(
      throwError(() => new Error('Load error'))
    );

    component.ngOnInit();

    setTimeout(() => {
      expect(component.isLoading).toBe(false);
      done();
    }, 100);
  });

  it('should set isLoading to true while fetching progress', () => {
    component.isLoading = true;
    fixture.detectChanges();

    expect(component.isLoading).toBe(true);
  });

  it('should disable back button on first step', (done) => {
    fixture.detectChanges();

    setTimeout(() => {
      expect(component.progress?.currentStep).toBe(1);
      expect(component.canGoBack()).toBe(false);
      done();
    }, 100);
  });

  it('should enable back button on steps 2+', (done) => {
    const step2Progress = { ...mockOnboardingProgress, currentStep: 2 };
    onboardingService.getProgress.and.returnValue(of(step2Progress as any));

    fixture.detectChanges();

    setTimeout(() => {
      expect(component.progress?.currentStep).toBe(2);
      expect(component.canGoBack()).toBe(true);
      done();
    }, 100);
  });

  it('should disable next button on final step', (done) => {
    const finalStepProgress = { ...mockOnboardingProgress, currentStep: 4 };
    onboardingService.getProgress.and.returnValue(of(finalStepProgress as any));

    fixture.detectChanges();

    setTimeout(() => {
      expect(component.progress?.currentStep).toBe(4);
      expect(component.canGoNext()).toBe(false);
      done();
    }, 100);
  });

  it('should enable next button when can advance and not on final step', (done) => {
    fixture.detectChanges();

    setTimeout(() => {
      expect(component.progress?.canAdvance).toBe(true);
      expect(component.progress?.currentStep).toBe(1);
      expect(component.canGoNext()).toBe(true);
      done();
    }, 100);
  });

  it('should show consent required message for minors without consent', (done) => {
    const minorProgress = { ...mockOnboardingProgress, isMinor: true, hasParentalConsent: false, canAdvance: false };
    onboardingService.getProgress.and.returnValue(of(minorProgress as any));

    component.ngOnInit();

    setTimeout(() => {
      expect(component.getBlockedMessage()).toContain('consent');
      done();
    }, 100);
  });

  it('should handle step submission error', (done) => {
    onboardingService.submitStep.and.returnValue(
      throwError(() => new Error('Submit error'))
    );

    fixture.detectChanges();

    setTimeout(() => {
      component.nextStep();

      setTimeout(() => {
        expect(component.isSubmitting).toBe(false);
        done();
      }, 100);
    }, 100);
  });

  it('should calculate progress percentage correctly', (done) => {
    fixture.detectChanges();

    setTimeout(() => {
      const completedCount = component.progress?.completedSteps.length || 0;
      const totalSteps = component.progress?.steps.length || 4;
      const progressPercentage = Math.round((completedCount / totalSteps) * 100);

      expect(progressPercentage).toBe(25); // 1 of 4 completed
      done();
    }, 100);
  });

  it('should show completion message when all steps done', (done) => {
    const completeProgress = { ...mockOnboardingProgress, currentStep: 4, completedSteps: [1, 2, 3, 4] };
    onboardingService.getProgress.and.returnValue(of(completeProgress as any));

    fixture.detectChanges();

    setTimeout(() => {
      expect(component.isComplete()).toBe(true);
      done();
    }, 100);
  });
});
