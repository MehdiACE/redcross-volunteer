import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { VolunteerService } from '../../../core/services/volunteer.service';
import { RegisterVolunteerDto } from '../../../core/models/volunteer.model';

@Component({
  selector: 'app-registration',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatChipsModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatIconModule,
    TranslateModule,
  ],
  templateUrl: './registration.component.html',
})
export class RegistrationComponent implements OnInit {
  registrationForm!: FormGroup;
  isLoading = false;
  hidePassword = true;
  currentStepIndex = 0;
  isDarkMode = false;
  currentLang = 'fr';
  stepTitles = [
    'registration.sections.personalInfo',
    'registration.sections.address',
    'registration.sections.emergencyContact',
    'registration.sections.preferences',
  ];
  availableInterests = [
    { value: 'First Aid', labelKey: 'registration.interests.firstAid' },
    { value: 'Disaster Response', labelKey: 'registration.interests.disasterResponse' },
    { value: 'Community Programs', labelKey: 'registration.interests.communityPrograms' },
    { value: 'Blood Drive', labelKey: 'registration.interests.bloodDrive' },
    { value: 'Emergency Services', labelKey: 'registration.interests.emergencyServices' },
    { value: 'Youth Programs', labelKey: 'registration.interests.youthPrograms' },
  ];
  daysOfWeek = [
    { value: 'Monday', labelKey: 'registration.days.monday' },
    { value: 'Tuesday', labelKey: 'registration.days.tuesday' },
    { value: 'Wednesday', labelKey: 'registration.days.wednesday' },
    { value: 'Thursday', labelKey: 'registration.days.thursday' },
    { value: 'Friday', labelKey: 'registration.days.friday' },
    { value: 'Saturday', labelKey: 'registration.days.saturday' },
    { value: 'Sunday', labelKey: 'registration.days.sunday' },
  ];

  constructor(
    private fb: FormBuilder,
    private volunteerService: VolunteerService,
    private router: Router,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.currentLang = this.translate.currentLang || 'fr';
    this.isDarkMode = document.documentElement.classList.contains('dark');
  }

  toggleTheme(): void {
    this.isDarkMode = !this.isDarkMode;
    document.documentElement.classList.toggle('dark', this.isDarkMode);
    localStorage.setItem('theme', this.isDarkMode ? 'dark' : 'light');
  }

  toggleLanguage(): void {
    this.currentLang = this.currentLang === 'fr' ? 'en' : 'fr';
    this.translate.use(this.currentLang);
    localStorage.setItem('language', this.currentLang);
  }

  private buildForm(): void {
    this.registrationForm = this.fb.group(
      {
        firstName: ['', [Validators.required, Validators.maxLength(100)]],
        lastName: ['', [Validators.required, Validators.maxLength(100)]],
        email: ['', [Validators.required, Validators.email, Validators.maxLength(255)]],
        password: [
          '',
          [
            Validators.required,
            Validators.minLength(8),
            Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).+$/),
          ],
        ],
        confirmPassword: ['', Validators.required],
        phoneCountryCode: ['+1'],
        phone: ['', [Validators.required, Validators.pattern(/^[0-9]{7,14}$/)]],
        dateOfBirth: ['', Validators.required],
        address: this.fb.group({
          street: ['', [Validators.required, Validators.maxLength(255)]],
          city: ['', [Validators.required, Validators.maxLength(100)]],
          stateProvince: ['', [Validators.required, Validators.maxLength(100)]],
          postalCode: ['', [Validators.required, Validators.maxLength(20)]],
          country: ['', [Validators.required, Validators.maxLength(100)]],
        }),
        emergencyContact: this.fb.group({
          name: ['', [Validators.required, Validators.maxLength(200)]],
          phone: ['', [Validators.required, Validators.pattern(/^\+?[1-9]\d{1,14}$/)]],
        }),
        areasOfInterest: [[], Validators.required],
        availability: this.fb.group({
          daysOfWeek: [[], Validators.required],
          timePreference: ['', Validators.required],
        }),
        languagePreference: ['fr', Validators.required],
      },
      { validators: this.passwordMatchValidator },
    );
  }

  private passwordMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  onSubmit(): void {
    if (this.registrationForm.invalid) {
      this.registrationForm.markAllAsTouched();
      this.snackBar.open(
        this.translate.instant('registration.errors.invalidForm'),
        this.translate.instant('common.close'),
        { duration: 5000 },
      );
      return;
    }

    this.isLoading = true;
    const formValue = this.registrationForm.value;

    const dto: RegisterVolunteerDto = {
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      email: formValue.email,
      password: formValue.password,
      phone: `${formValue.phoneCountryCode}${formValue.phone.replace(/\D/g, '')}`,
      dateOfBirth: formValue.dateOfBirth,
      addressStreet: formValue.address.street,
      addressCity: formValue.address.city,
      addressStateProvince: formValue.address.stateProvince,
      addressPostalCode: formValue.address.postalCode,
      addressCountry: formValue.address.country,
      emergencyContactName: formValue.emergencyContact.name,
      emergencyContactPhone: formValue.emergencyContact.phone,
      areasOfInterest: formValue.areasOfInterest,
      availability: {
        daysOfWeek: formValue.availability.daysOfWeek,
        timePreference: formValue.availability.timePreference,
      },
      languagePreference: formValue.languagePreference,
    };

    this.volunteerService.register(dto).subscribe({
      next: (response) => {
        // Store auth token and userId for authentication
        localStorage.setItem('authToken', response.accessToken);
        localStorage.setItem('userId', response.userId);
        localStorage.setItem('userName', `${formValue.firstName} ${formValue.lastName}`.trim());
        this.snackBar.open(
          this.translate.instant('registration.success'),
          this.translate.instant('common.close'),
          { duration: 5000 },
        );
        this.router.navigate(['/onboarding']);
      },
      error: (error) => {
        this.isLoading = false;
        const message =
          error.status === 409
            ? this.translate.instant('registration.errors.emailExists')
            : this.translate.instant('registration.errors.serverError');
        this.snackBar.open(message, this.translate.instant('common.close'), { duration: 5000 });
      },
    });
  }

  getErrorMessage(fieldName: string): string {
    const control = this.registrationForm.get(fieldName);
    if (!control || !control.errors) return '';

    if (control.errors['required']) {
      return this.translate.instant('registration.errors.required');
    }
    if (control.errors['email']) {
      return this.translate.instant('registration.errors.invalidEmail');
    }
    if (control.errors['pattern']) {
      return this.translate.instant('registration.errors.invalidPhone');
    }
    if (control.errors['maxlength']) {
      return this.translate.instant('registration.errors.maxLength', {
        max: control.errors['maxlength'].requiredLength,
      });
    }
    return '';
  }

  onStepChange(event: any): void {
    this.currentStepIndex = event.selectedIndex ?? 0;
  }

  goToStep(index: number): void {
    this.currentStepIndex = index;
  }

  nextStep(): void {
    if (this.currentStepIndex < this.stepTitles.length - 1) {
      this.currentStepIndex++;
    }
  }

  previousStep(): void {
    if (this.currentStepIndex > 0) {
      this.currentStepIndex--;
    }
  }
}
