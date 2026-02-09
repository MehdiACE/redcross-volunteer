import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { of, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { RegistrationComponent } from './registration.component';
import { VolunteerService } from '../../../core/services/volunteer.service';
import { TranslateFakeLoader, TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { RouterTestingModule } from '@angular/router/testing';

describe('RegistrationComponent', () => {
  let component: RegistrationComponent;
  let fixture: ComponentFixture<RegistrationComponent>;
  let volunteerService: jasmine.SpyObj<VolunteerService>;
  let router: Router;

  beforeEach(async () => {
    const volunteerServiceSpy = jasmine.createSpyObj('VolunteerService', ['register']);
    await TestBed.configureTestingModule({
      imports: [
        RegistrationComponent,
        ReactiveFormsModule,
        MatSnackBarModule,
        MatDatepickerModule,
        MatNativeDateModule,
        RouterTestingModule,
        TranslateModule.forRoot({
          loader: { provide: TranslateLoader, useClass: TranslateFakeLoader },
        }),
      ],
      providers: [{ provide: VolunteerService, useValue: volunteerServiceSpy }],
    }).compileComponents();

    volunteerService = TestBed.inject(VolunteerService) as jasmine.SpyObj<VolunteerService>;
    router = TestBed.inject(Router);
    spyOn(router, 'navigate');

    fixture = TestBed.createComponent(RegistrationComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with required validators', () => {
    fixture.detectChanges();

    const form = component.registrationForm;
    expect(form.get('firstName')).toBeTruthy();
    expect(form.get('lastName')).toBeTruthy();
    expect(form.get('email')).toBeTruthy();
    expect(form.get('phone')).toBeTruthy();
    expect(form.get('dateOfBirth')).toBeTruthy();
    expect(form.get('languagePreference')).toBeTruthy();
    expect(form.get('areasOfInterest')).toBeTruthy();
    expect(form.get('availability')).toBeTruthy();
  });

  it('should mark email field as invalid with empty value', () => {
    fixture.detectChanges();

    const emailControl = component.registrationForm.get('email');
    emailControl?.setValue('');
    emailControl?.markAsTouched();

    expect(emailControl?.hasError('required')).toBe(true);
  });

  it('should mark email field as invalid with incorrect format', () => {
    fixture.detectChanges();

    const emailControl = component.registrationForm.get('email');
    emailControl?.setValue('invalid-email');
    emailControl?.markAsTouched();

    expect(emailControl?.hasError('email')).toBe(true);
  });

  it('should mark email field as valid with correct format', () => {
    fixture.detectChanges();

    const emailControl = component.registrationForm.get('email');
    emailControl?.setValue('test@example.com');

    expect(emailControl?.valid).toBe(true);
    expect(emailControl?.hasError('email')).toBe(false);
  });

  it('should mark phone field as invalid with incorrect format', () => {
    fixture.detectChanges();

    const phoneControl = component.registrationForm.get('phone');
    phoneControl?.setValue('invalid-phone');
    phoneControl?.markAsTouched();

    expect(phoneControl?.hasError('pattern')).toBe(true);
  });

  it('should mark phone field as valid with correct format', () => {
    fixture.detectChanges();

    const phoneControl = component.registrationForm.get('phone');
    phoneControl?.setValue('+15145551234');

    expect(phoneControl?.valid).toBe(true);
  });

  it('should mark firstName and lastName as required', () => {
    fixture.detectChanges();

    const firstNameControl = component.registrationForm.get('firstName');
    const lastNameControl = component.registrationForm.get('lastName');

    firstNameControl?.setValue('');
    firstNameControl?.markAsTouched();
    lastNameControl?.setValue('');
    lastNameControl?.markAsTouched();

    expect(firstNameControl?.hasError('required')).toBe(true);
    expect(lastNameControl?.hasError('required')).toBe(true);
  });

  it('should enable submit button when form is valid', () => {
    fixture.detectChanges();

    const form = component.registrationForm;
    form.patchValue({
      firstName: 'John',
      lastName: 'Doe',
      email: 'john@example.com',
      password: 'SecurePassword123!',
      confirmPassword: 'SecurePassword123!',
      phone: '+15145551234',
      dateOfBirth: new Date('2000-01-01'),
      languagePreference: 'en',
      areasOfInterest: ['First Aid'],
      availability: {
        daysOfWeek: ['Monday'],
        timePreference: 'Morning',
      },
      address: {
        street: '123 Main St',
        city: 'Montreal',
        stateProvince: 'QC',
        postalCode: 'H1A 1A1',
        country: 'Canada',
      },
      emergencyContact: {
        name: 'Jane Doe',
        phone: '+15145555678',
      },
    });

    expect(form.valid).toBe(true);
  });

  it('should disable submit button when form is invalid', () => {
    fixture.detectChanges();

    const form = component.registrationForm;
    form.patchValue({
      firstName: 'John',
      email: 'invalid-email',
    });

    expect(form.valid).toBe(false);
  });

  it('should call volunteerService.register on submit', () => {
    volunteerService.register.and.returnValue(
      of({ accessToken: 'token', userId: 'user-1', roles: [] } as any),
    );

    fixture.detectChanges();

    const form = component.registrationForm;
    form.patchValue({
      firstName: 'John',
      lastName: 'Doe',
      email: 'john@example.com',
      password: 'SecurePassword123!',
      confirmPassword: 'SecurePassword123!',
      phone: '+15145551234',
      dateOfBirth: new Date('2000-01-01'),
      languagePreference: 'en',
      areasOfInterest: ['First Aid'],
      availability: {
        daysOfWeek: ['Monday'],
        timePreference: 'Morning',
      },
      address: {
        street: '123 Main St',
        city: 'Montreal',
        stateProvince: 'QC',
        postalCode: 'H1A 1A1',
        country: 'Canada',
      },
      emergencyContact: {
        name: 'Jane Doe',
        phone: '+15145555678',
      },
    });

    component.onSubmit();

    expect(volunteerService.register).toHaveBeenCalledWith(
      jasmine.objectContaining({
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
      }),
    );
  });

  it('should navigate on successful registration', (done) => {
    volunteerService.register.and.returnValue(
      of({ accessToken: 'token', userId: 'user-1', roles: [] } as any),
    );

    fixture.detectChanges();

    const form = component.registrationForm;
    form.patchValue({
      firstName: 'John',
      lastName: 'Doe',
      email: 'john@example.com',
      password: 'SecurePassword123!',
      confirmPassword: 'SecurePassword123!',
      phone: '+15145551234',
      dateOfBirth: new Date('2000-01-01'),
      languagePreference: 'en',
      areasOfInterest: ['First Aid'],
      availability: {
        daysOfWeek: ['Monday'],
        timePreference: 'Morning',
      },
      address: {
        street: '123 Main St',
        city: 'Montreal',
        stateProvince: 'QC',
        postalCode: 'H1A 1A1',
        country: 'Canada',
      },
      emergencyContact: {
        name: 'Jane Doe',
        phone: '+15145555678',
      },
    });

    component.onSubmit();

    setTimeout(() => {
      expect(router.navigate).toHaveBeenCalledWith(['/onboarding']);
      done();
    }, 100);
  });

  it('should handle registration error', (done) => {
    volunteerService.register.and.returnValue(throwError(() => new Error('Duplicate email')));

    fixture.detectChanges();

    const form = component.registrationForm;
    form.patchValue({
      firstName: 'John',
      lastName: 'Doe',
      email: 'john@example.com',
      password: 'SecurePassword123!',
      confirmPassword: 'SecurePassword123!',
      phone: '+15145551234',
      dateOfBirth: new Date('2000-01-01'),
      languagePreference: 'en',
      areasOfInterest: ['First Aid'],
      availability: {
        daysOfWeek: ['Monday'],
        timePreference: 'Morning',
      },
      address: {
        street: '123 Main St',
        city: 'Montreal',
        stateProvince: 'QC',
        postalCode: 'H1A 1A1',
        country: 'Canada',
      },
      emergencyContact: {
        name: 'Jane Doe',
        phone: '+15145555678',
      },
    });

    component.onSubmit();

    setTimeout(() => {
      expect(component.isLoading).toBe(false);
      done();
    }, 100);
  });

  it('should set isLoading to true during registration', () => {
    volunteerService.register.and.returnValue(
      of({ accessToken: 'token', userId: 'user-1', roles: [] } as any),
    );

    fixture.detectChanges();

    const form = component.registrationForm;
    form.patchValue({
      firstName: 'John',
      lastName: 'Doe',
      email: 'john@example.com',
      password: 'SecurePassword123!',
      confirmPassword: 'SecurePassword123!',
      phone: '+15145551234',
      dateOfBirth: new Date('2000-01-01'),
      languagePreference: 'en',
      areasOfInterest: ['First Aid'],
      availability: {
        daysOfWeek: ['Monday'],
        timePreference: 'Morning',
      },
      address: {
        street: '123 Main St',
        city: 'Montreal',
        stateProvince: 'QC',
        postalCode: 'H1A 1A1',
        country: 'Canada',
      },
      emergencyContact: {
        name: 'Jane Doe',
        phone: '+15145555678',
      },
    });

    component.onSubmit();
    expect(component.isLoading).toBe(true);
  });

  it('should allow multiple interests selection', () => {
    fixture.detectChanges();

    const interestsControl = component.registrationForm.get('areasOfInterest');
    interestsControl?.setValue(['First Aid', 'Disaster Response', 'Youth Programs']);

    expect(interestsControl?.value).toEqual(['First Aid', 'Disaster Response', 'Youth Programs']);
    expect(interestsControl?.valid).toBe(true);
  });

  it('should allow multiple availability days selection', () => {
    fixture.detectChanges();

    const availabilityDays = component.registrationForm.get('availability.daysOfWeek');
    availabilityDays?.setValue(['Monday', 'Wednesday', 'Saturday']);

    expect(availabilityDays?.value).toEqual(['Monday', 'Wednesday', 'Saturday']);
    expect(availabilityDays?.valid).toBe(true);
  });

  it('should support both language preferences', () => {
    fixture.detectChanges();

    const langControl = component.registrationForm.get('languagePreference');

    langControl?.setValue('en');
    expect(langControl?.valid).toBe(true);

    langControl?.setValue('fr');
    expect(langControl?.valid).toBe(true);
  });

  it('should require at least one interest', () => {
    fixture.detectChanges();

    const interestsControl = component.registrationForm.get('areasOfInterest');
    interestsControl?.setValue([]);
    interestsControl?.markAsTouched();

    expect(interestsControl?.hasError('required')).toBe(true);
  });

  it('should require at least one availability day', () => {
    fixture.detectChanges();

    const availabilityDays = component.registrationForm.get('availability.daysOfWeek');
    availabilityDays?.setValue([]);
    availabilityDays?.markAsTouched();

    expect(availabilityDays?.hasError('required')).toBe(true);
  });
});
