import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
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
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ConsentRequestDto, ConsentService, SubmitConsentDto } from '../../../core/services/consent.service';

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
export class GuardianConsentComponent implements OnInit, OnDestroy, AfterViewInit {
  consentForm!: FormGroup;
  isLoading = false;
  isSubmitting = false;
  consentData: ConsentRequestDto | null = null;
  isSignatureEmpty = true;

  @ViewChild('signatureCanvas') signatureCanvas?: ElementRef<HTMLCanvasElement>;

  private signatureContext: CanvasRenderingContext2D | null = null;
  private isDrawing = false;
  private volunteerId: string | null = null;
  private destroy$ = new Subject<void>();

  constructor(
    private formBuilder: FormBuilder,
    private consentService: ConsentService,
    private snackBar: MatSnackBar,
    private route: ActivatedRoute,
    private translate: TranslateService
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    this.volunteerId = this.route.snapshot.paramMap.get('volunteerId');
    if (!this.volunteerId) {
      this.snackBar.open(
        this.translate.instant('guardianConsent.errors.missingVolunteer'),
        this.translate.instant('common.close'),
        { duration: 5000 }
      );
      return;
    }

    this.loadConsentRequest(this.volunteerId);
  }

  ngAfterViewInit(): void {
    this.initializeSignaturePad();
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

  private loadConsentRequest(volunteerId: string): void {
    this.isLoading = true;
    this.consentService.getConsentRequest(volunteerId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.consentData = data;
          this.populateVolunteerInfo(data);
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Failed to load consent request:', error);
          this.snackBar.open(
            this.translate.instant('guardianConsent.errors.loadFailed'),
            this.translate.instant('common.close'),
            { duration: 5000 }
          );
          this.isLoading = false;
        }
      });
  }

  private populateVolunteerInfo(data: ConsentRequestDto): void {
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

  private initializeSignaturePad(): void {
    const canvas = this.signatureCanvas?.nativeElement;
    if (!canvas) return;

    const container = canvas.parentElement;
    const width = container ? container.getBoundingClientRect().width : 600;
    canvas.width = Math.max(300, Math.floor(width));
    canvas.height = 200;

    this.signatureContext = canvas.getContext('2d');
    if (this.signatureContext) {
      this.signatureContext.lineWidth = 2;
      this.signatureContext.lineCap = 'round';
      this.signatureContext.strokeStyle = '#111827';
    }
  }

  startDrawing(event: MouseEvent | TouchEvent): void {
    event.preventDefault();
    if (!this.signatureContext || !this.signatureCanvas) return;

    const point = this.getCanvasPoint(event);
    if (!point) return;

    this.isDrawing = true;
    this.signatureContext.beginPath();
    this.signatureContext.moveTo(point.x, point.y);
  }

  draw(event: MouseEvent | TouchEvent): void {
    if (!this.isDrawing || !this.signatureContext) return;
    event.preventDefault();

    const point = this.getCanvasPoint(event);
    if (!point) return;

    this.signatureContext.lineTo(point.x, point.y);
    this.signatureContext.stroke();
    this.isSignatureEmpty = false;
  }

  endDrawing(): void {
    if (!this.isDrawing) return;
    this.isDrawing = false;
    this.signatureContext?.closePath();
  }

  clearSignature(): void {
    const canvas = this.signatureCanvas?.nativeElement;
    if (!canvas || !this.signatureContext) return;

    this.signatureContext.clearRect(0, 0, canvas.width, canvas.height);
    this.isSignatureEmpty = true;
  }

  private getCanvasPoint(event: MouseEvent | TouchEvent): { x: number; y: number } | null {
    const canvas = this.signatureCanvas?.nativeElement;
    if (!canvas) return null;

    const rect = canvas.getBoundingClientRect();
    if (event instanceof MouseEvent) {
      return {
        x: event.clientX - rect.left,
        y: event.clientY - rect.top
      };
    }

    const touch = event.touches[0] || event.changedTouches[0];
    if (!touch) return null;

    return {
      x: touch.clientX - rect.left,
      y: touch.clientY - rect.top
    };
  }

  submit(): void {
    if (!this.volunteerId) {
      this.snackBar.open(
        this.translate.instant('guardianConsent.errors.missingVolunteer'),
        this.translate.instant('common.close'),
        { duration: 5000 }
      );
      return;
    }

    if (!this.consentForm.valid || this.isSignatureEmpty) {
      this.snackBar.open(
        this.translate.instant('guardianConsent.errors.signatureRequired'),
        this.translate.instant('common.close'),
        { duration: 5000 }
      );
      return;
    }

    const canvas = this.signatureCanvas?.nativeElement;
    const signature = canvas ? canvas.toDataURL('image/png') : '';

    const formData: SubmitConsentDto = {
      guardianInfo: this.consentForm.get('guardianInfo')?.value,
      guardianAgreement: this.consentForm.get('guardianAgreement')?.value,
      dataProcessingAgreement: this.consentForm.get('dataProcessingAgreement')?.value,
      signature
    };

    this.isSubmitting = true;

    this.consentService.submitConsent(this.volunteerId, formData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open(
            this.translate.instant('guardianConsent.messages.submitSuccess'),
            this.translate.instant('common.close'),
            { duration: 3000 }
          );
          this.isSubmitting = false;
        },
        error: (error) => {
          console.error('Failed to submit consent:', error);
          this.snackBar.open(
            this.translate.instant('guardianConsent.errors.submitFailed'),
            this.translate.instant('common.close'),
            { duration: 5000 }
          );
          this.isSubmitting = false;
        }
      });
  }

  getFormattedDate(date: string | null): string {
    if (!date) return '';
    return new Date(date).toLocaleDateString();
  }
}
