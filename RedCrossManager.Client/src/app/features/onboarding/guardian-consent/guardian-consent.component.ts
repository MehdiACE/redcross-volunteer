import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { TranslateModule } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ConsentService } from '../../../core/services/consent.service';

@Component({
  selector: 'app-guardian-consent',
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
    MatDividerModule
  ],
  templateUrl: './guardian-consent.component.html',
  styleUrls: ['./guardian-consent.component.scss']
})
export class GuardianConsentComponent implements OnInit, OnDestroy {
  consentForm!: FormGroup;
  isLoading = false;
  isSubmitting = false;
  consentData: any;
  showSignaturePad = false;
  signaturePadRef: any;
  isSignatureEmpty = true;

  private destroy$ = new Subject<void>();

  constructor(
    private formBuilder: FormBuilder,
    private consentService: ConsentService,
    private snackBar: MatSnackBar
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    this.loadConsentRequest();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeForm(): void {
    this.consentForm = this.formBuilder.group({
      guardianAgreement: [false, Validators.requiredTrue],
      dataProcessingAgreement: [false, Validators.requiredTrue],
      volunteerInfo: this.formBuilder.group({
        firstName: [{ value: '', disabled: true }],
        lastName: [{ value: '', disabled: true }],
        dateOfBirth: [{ value: '', disabled: true }],
        email: [{ value: '', disabled: true }]
      }),
      guardianInfo: this.formBuilder.group({
        fullName: ['', Validators.required],
        email: ['', [Validators.required, Validators.email]],
        phone: ['', Validators.required],
        relationship: ['', Validators.required]
      }),
      acceptTerms: [false, Validators.requiredTrue]
    });
  }

  private loadConsentRequest(): void {
    this.isLoading = true;
    this.consentService.getConsentRequest()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.consentData = data;
          this.populateVolunteerInfo(data);
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Failed to load consent request:', error);
          this.snackBar.open('Failed to load consent request', 'Close', { duration: 5000 });
          this.isLoading = false;
        }
      });
  }

  private populateVolunteerInfo(data: any): void {
    const volunteerGroup = this.consentForm.get('volunteerInfo');
    if (volunteerGroup) {
      volunteerGroup.patchValue({
        firstName: data.volunteer?.firstName || '',
        lastName: data.volunteer?.lastName || '',
        dateOfBirth: data.volunteer?.dateOfBirth || '',
        email: data.volunteer?.email || ''
      });
    }

    // Pre-fill guardian email if available
    const guardianGroup = this.consentForm.get('guardianInfo');
    if (guardianGroup && data.guardianEmail) {
      guardianGroup.patchValue({
        email: data.guardianEmail
      });
    }
  }

  onSignatureChange(isEmpty: boolean): void {
    this.isSignatureEmpty = isEmpty;
  }

  clearSignature(): void {
    if (this.signaturePadRef) {
      this.signaturePadRef.clear();
      this.isSignatureEmpty = true;
    }
  }

  submit(): void {
    if (!this.consentForm.valid || this.isSignatureEmpty) {
      this.snackBar.open('Please complete all required fields and provide a signature', 'Close', { duration: 5000 });
      return;
    }

    this.isSubmitting = true;

    const formData = {
      consentId: this.consentData?.id,
      guardianInfo: this.consentForm.get('guardianInfo')?.value,
      guardianAgreement: this.consentForm.get('guardianAgreement')?.value,
      dataProcessingAgreement: this.consentForm.get('dataProcessingAgreement')?.value,
      signature: this.signaturePadRef?.toDataURL() || ''
    };

    this.consentService.submitConsent(formData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('Consent submitted successfully', 'Close', { duration: 3000 });
          this.isSubmitting = false;
          // Navigate back or show completion message
        },
        error: (error) => {
          console.error('Failed to submit consent:', error);
          this.snackBar.open('Failed to submit consent', 'Close', { duration: 5000 });
          this.isSubmitting = false;
        }
      });
  }

  getFormattedDate(date: string | null): string {
    if (!date) return '';
    return new Date(date).toLocaleDateString();
  }
}
