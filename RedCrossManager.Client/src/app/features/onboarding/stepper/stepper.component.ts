import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatStepperModule } from '@angular/material/stepper';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { OnboardingService } from '../../../core/services/onboarding.service';
import { OnboardingProgressDto } from '../../../core/models/onboarding.model';

@Component({
  selector: 'app-stepper',
  standalone: true,
  imports: [
    CommonModule,
    MatStepperModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatCardModule,
    MatChipsModule,
    RouterLink,
    TranslateModule
  ],
  templateUrl: './stepper.component.html',
  styleUrls: ['./stepper.component.scss']
})
export class StepperComponent implements OnInit, OnDestroy {
  progress: OnboardingProgressDto | null = null;
  isLoading = true;
  isSubmitting = false;
  selectedStepIndex = 0;
  private destroy$ = new Subject<void>();
  private volunteerId: string = '';

  stepTitles = [
    'onboarding.steps.personalInfo',
    'onboarding.steps.background',
    'onboarding.steps.training',
    'onboarding.steps.assignment'
  ];

  stepDescriptions = [
    'onboarding.steps.personalInfoDesc',
    'onboarding.steps.backgroundDesc',
    'onboarding.steps.trainingDesc',
    'onboarding.steps.assignmentDesc'
  ];

  constructor(
    private onboardingService: OnboardingService,
    private router: Router,
    private snackBar: MatSnackBar,
    private translate: TranslateService
  ) {
    // Get volunteerId from session/localStorage
    this.volunteerId = sessionStorage.getItem('volunteerId') || '';
  }

  ngOnInit(): void {
    if (!this.volunteerId) {
      this.router.navigate(['/register']);
      return;
    }

    this.loadProgress();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadProgress(): void {
    this.isLoading = true;
    this.onboardingService.getProgress(this.volunteerId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (progress) => {
          this.progress = progress;
          this.isLoading = false;
          // Set initial step index based on progress
          this.selectedStepIndex = this.getNextIncompleteStepIndex();
        },
        error: (error) => {
          this.isLoading = false;
          this.snackBar.open(
            this.translate.instant('onboarding.errors.loadFailed'),
            this.translate.instant('common.close'),
            { duration: 5000 }
          );
          console.error('Failed to load progress:', error);
        }
      });
  }

  private getNextIncompleteStepIndex(): number {
    if (!this.progress?.steps) return 0;
    const index = this.progress.steps.findIndex(s => s.status === 'Pending');
    return index >= 0 ? index : this.progress.steps.length - 1;
  }

  canAdvance(stepIndex: number): boolean {
    if (!this.progress?.steps) return false;
    // Can advance if current step is completed or minor without parental consent yet
    const currentStep = this.progress.steps[stepIndex];
    if (currentStep.status === 'Completed') return true;
    if (stepIndex === 0) return currentStep.status === 'Submitted'; // Personal info must be submitted
    return false;
  }

  isStepLocked(stepIndex: number): boolean {
    if (!this.progress) return true;

    // Minor volunteers are locked at step 2 (background) until parental consent is approved
    if (this.progress.isMinor && stepIndex >= 1 && !this.progress.parentalConsentApproved) {
      return true;
    }

    // Steps are locked if previous step not submitted/completed
    if (stepIndex === 0) return false;
    const prevStep = this.progress.steps?.[stepIndex - 1];
    return prevStep ? prevStep.status !== 'Submitted' && prevStep.status !== 'Completed' : true;
  }

  getStepStatus(stepIndex: number): string {
    if (!this.progress?.steps?.[stepIndex]) return 'pending';
    return this.progress.steps[stepIndex].status.toLowerCase();
  }

  getStepStatusLabel(stepIndex: number): string {
    const status = this.getStepStatus(stepIndex);
    return `onboarding.stepStatus.${status}`;
  }

  onStepChange(event: any): void {
    if (this.isStepLocked(event.selectedIndex)) {
      event.previouslySelectedIndex = this.selectedStepIndex;
      this.snackBar.open(
        this.translate.instant('onboarding.errors.stepLocked'),
        this.translate.instant('common.close'),
        { duration: 3000 }
      );
    } else {
      this.selectedStepIndex = event.selectedIndex;
    }
  }

  submitStep(stepIndex: number): void {
    if (!this.progress?.steps?.[stepIndex]) return;

    const step = this.progress.steps[stepIndex];
    this.isSubmitting = true;

    this.onboardingService.submitStep(this.volunteerId, step.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open(
            this.translate.instant('onboarding.messages.stepSubmitted'),
            this.translate.instant('common.close'),
            { duration: 3000 }
          );
          this.isSubmitting = false;
          this.loadProgress(); // Refresh progress
          // Auto-advance to next step if available
          if (stepIndex < (this.progress?.steps?.length || 0) - 1) {
            setTimeout(() => {
              this.selectedStepIndex = stepIndex + 1;
            }, 1000);
          }
        },
        error: (error) => {
          this.isSubmitting = false;
          const message = error.status === 409
            ? this.translate.instant('onboarding.errors.stepAlreadySubmitted')
            : this.translate.instant('onboarding.errors.submitFailed');
          this.snackBar.open(message, this.translate.instant('common.close'), { duration: 5000 });
        }
      });
  }

  isStepCompleted(stepIndex: number): boolean {
    return this.getStepStatus(stepIndex) === 'completed';
  }

  getCompletedStepsCount(): number {
    return this.progress?.steps?.filter(s => s.status === 'Completed').length || 0;
  }

  getTotalSteps(): number {
    return this.progress?.steps?.length || 0;
  }

  getProgressPercentage(): number {
    const total = this.getTotalSteps();
    if (total === 0) return 0;
    return Math.round((this.getCompletedStepsCount() / total) * 100);
  }

  getMinorWarningMessage(): string {
    return this.progress?.isMinor && !this.progress?.parentalConsentApproved
      ? 'onboarding.warnings.minorConsentRequired'
      : '';
  }

  getOverallStatus(): string {
    if (!this.progress) return 'unknown';
    const completed = this.getCompletedStepsCount();
    const total = this.getTotalSteps();
    if (completed === 0) return 'notStarted';
    if (completed === total) return 'completed';
    return 'inProgress';
  }

  resumeProgress(): void {
    this.loadProgress();
  }

  goToRegistration(): void {
    this.router.navigate(['/register']);
  }
}
