import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { VolunteerService } from '../../core/services/volunteer.service';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss'],
})
export class ProfileComponent implements OnInit, OnDestroy {
  profileForm!: FormGroup;
  isLoading = false;
  isSaving = false;
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private volunteerService: VolunteerService,
    private snackBar: MatSnackBar,
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    this.loadProfile();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeForm(): void {
    this.profileForm = this.fb.group({
      firstName: [{ value: '', disabled: true }],
      lastName: [{ value: '', disabled: true }],
      email: [{ value: '', disabled: true }],
      phone: [{ value: '', disabled: true }],
      status: [{ value: '', disabled: true }],
      languagePreference: [{ value: '', disabled: true }],
      smsOptIn: [false],
    });
  }

  private loadProfile(): void {
    this.isLoading = true;
    this.volunteerService
      .getProfile()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (profile) => {
          this.profileForm.patchValue({
            firstName: profile.firstName,
            lastName: profile.lastName,
            email: profile.email,
            phone: profile.phone,
            status: profile.status,
            languagePreference: profile.languagePreference,
            smsOptIn: profile.smsOptIn,
          });
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading profile:', error);
          this.snackBar.open('Error loading profile', 'Close', {
            duration: 5000,
            panelClass: ['error-snackbar'],
          });
          this.isLoading = false;
        },
      });
  }

  saveSmsPreference(): void {
    if (this.profileForm.invalid) {
      return;
    }

    this.isSaving = true;
    const smsOptIn = this.profileForm.get('smsOptIn')?.value ?? false;

    this.volunteerService
      .updateSmsOptIn(smsOptIn)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open(
            smsOptIn ? 'SMS notifications enabled' : 'SMS notifications disabled',
            'Close',
            { duration: 3000, panelClass: ['success-snackbar'] },
          );
          this.isSaving = false;
        },
        error: (error) => {
          console.error('Error updating SMS preference:', error);
          this.snackBar.open('Error updating SMS preference', 'Close', {
            duration: 5000,
            panelClass: ['error-snackbar'],
          });
          this.isSaving = false;
          // Revert the checkbox
          this.loadProfile();
        },
      });
  }
}
