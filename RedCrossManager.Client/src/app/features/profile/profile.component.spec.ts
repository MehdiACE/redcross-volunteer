import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { TranslateFakeLoader, TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { of, throwError } from 'rxjs';
import { ProfileComponent } from './profile.component';
import { VolunteerService } from '../../core/services/volunteer.service';
import { VolunteerDto } from '../../core/models/volunteer.model';

describe('ProfileComponent', () => {
  let component: ProfileComponent;
  let fixture: ComponentFixture<ProfileComponent>;
  let volunteerService: jasmine.SpyObj<VolunteerService>;

  const mockVolunteer: VolunteerDto = {
    id: '123',
    firstName: 'John',
    lastName: 'Doe',
    email: 'john@example.com',
    phone: '+15145551234',
    dateOfBirth: new Date('1990-01-01'),
    status: 'Active',
    languagePreference: 'en',
    registeredAt: new Date(),
    isMinor: false,
    smsOptIn: false,
  };

  beforeEach(async () => {
    const volunteerServiceSpy = jasmine.createSpyObj('VolunteerService', [
      'getProfile',
      'updateSmsOptIn',
    ]);

    await TestBed.configureTestingModule({
      imports: [
        ProfileComponent,
        ReactiveFormsModule,
        MatSnackBarModule,
        TranslateModule.forRoot({
          loader: { provide: TranslateLoader, useClass: TranslateFakeLoader },
        }),
      ],
      providers: [{ provide: VolunteerService, useValue: volunteerServiceSpy }],
    }).compileComponents();

    volunteerService = TestBed.inject(VolunteerService) as jasmine.SpyObj<VolunteerService>;
    volunteerService.getProfile.and.returnValue(of(mockVolunteer));
    volunteerService.updateSmsOptIn.and.returnValue(of(mockVolunteer));

    fixture = TestBed.createComponent(ProfileComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load profile on init', (done) => {
    fixture.detectChanges();

    expect(volunteerService.getProfile).toHaveBeenCalled();

    setTimeout(() => {
      expect(component.profileForm.get('firstName')?.value).toBe('John');
      expect(component.profileForm.get('lastName')?.value).toBe('Doe');
      expect(component.profileForm.get('email')?.value).toBe('john@example.com');
      expect(component.profileForm.get('smsOptIn')?.value).toBe(false);
      done();
    }, 100);
  });

  it('should update SMS opt-in', (done) => {
    fixture.detectChanges();

    setTimeout(() => {
      component.profileForm.patchValue({ smsOptIn: true });
      component.saveSmsPreference();

      expect(volunteerService.updateSmsOptIn).toHaveBeenCalledWith(true);
      done();
    }, 100);
  });

  it('should handle profile loading error', (done) => {
    volunteerService.getProfile.and.returnValue(throwError(() => new Error('Load error')));

    component.ngOnInit();

    setTimeout(() => {
      expect(component.isLoading).toBe(false);
      done();
    }, 100);
  });

  it('should handle SMS update error', (done) => {
    volunteerService.updateSmsOptIn.and.returnValue(throwError(() => new Error('Update error')));

    fixture.detectChanges();

    setTimeout(() => {
      component.profileForm.patchValue({ smsOptIn: true });
      component.saveSmsPreference();

      setTimeout(() => {
        expect(component.isSaving).toBe(false);
        done();
      }, 100);
    }, 100);
  });
});
