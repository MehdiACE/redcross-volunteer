import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MissionsComponent } from './missions.component';
import { MissionService } from '../../core/services/mission.service';
import { AuthService } from '../../core/services/auth.service';
import { TranslateModule } from '@ngx-translate/core';
import { of, throwError } from 'rxjs';
import { MissionDto } from '../../core/models/mission.model';

describe('MissionsComponent', () => {
  let component: MissionsComponent;
  let fixture: ComponentFixture<MissionsComponent>;
  let missionService: jasmine.SpyObj<MissionService>;
  let authService: jasmine.SpyObj<AuthService>;

  const mockMissions: MissionDto[] = [
    {
      id: 'mission-1',
      title: 'Blood Drive Support',
      description: 'Assist with donor intake',
      missionType: 'BloodDrive',
      location: 'Montreal Center',
      startAt: new Date(),
      endAt: new Date(),
      requiredCertifications: ['FirstAid'],
      volunteersNeeded: 5,
      travelBufferMinutes: 120,
      published: true,
      createdAt: new Date(),
      createdBy: 'coordinator-1',
      availableSlots: 2,
    },
    {
      id: 'mission-2',
      title: 'Community Program',
      description: 'Support community outreach',
      missionType: 'CommunityProgram',
      location: 'Quebec City',
      startAt: new Date(),
      endAt: new Date(),
      requiredCertifications: [],
      volunteersNeeded: 3,
      travelBufferMinutes: 120,
      published: true,
      createdAt: new Date(),
      createdBy: 'coordinator-2',
      availableSlots: 0,
    },
  ];

  beforeEach(async () => {
    const missionSpy = jasmine.createSpyObj('MissionService', ['getMissions', 'applyToMission']);
    const authSpy = jasmine.createSpyObj('AuthService', ['isAuthenticated', 'getUserId']);

    await TestBed.configureTestingModule({
      imports: [MissionsComponent, TranslateModule.forRoot()],
      providers: [
        { provide: MissionService, useValue: missionSpy },
        { provide: AuthService, useValue: authSpy },
      ],
    }).compileComponents();

    missionService = TestBed.inject(MissionService) as jasmine.SpyObj<MissionService>;
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;

    fixture = TestBed.createComponent(MissionsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load missions on init', () => {
    missionService.getMissions.and.returnValue(of(mockMissions));

    component.useMockData = false;
    component.ngOnInit();

    expect(missionService.getMissions).toHaveBeenCalled();
    expect(component.missions.length).toBe(2);
    expect(component.filteredMissions.length).toBe(2);
    expect(component.isLoading).toBeFalse();
  });

  it('should handle load error', () => {
    missionService.getMissions.and.returnValue(throwError(() => new Error('fail')));

    component.useMockData = false;
    component.ngOnInit();

    expect(component.loadError).toBeTrue();
    expect(component.isLoading).toBeFalse();
  });

  it('should filter missions by type', () => {
    component.missions = mockMissions;
    component.typeFilter = 'BloodDrive';

    component.applyFilters();

    expect(component.filteredMissions.length).toBe(1);
    expect(component.filteredMissions[0].missionType).toBe('BloodDrive');
  });

  it('should apply to mission when authenticated', () => {
    missionService.applyToMission.and.returnValue(
      of({
        id: 'assignment-1',
        missionId: 'mission-1',
        volunteerId: 'volunteer-1',
        status: 'Pending',
        roleDescription: undefined,
        assignedAt: new Date(),
        reminderSentAt: null,
      }),
    );

    authService.isAuthenticated.and.returnValue(true);
    authService.getUserId.and.returnValue('volunteer-1');

    component.applyToMission(mockMissions[0]);

    expect(missionService.applyToMission).toHaveBeenCalledWith('mission-1', {
      volunteerId: 'volunteer-1',
    });
  });
});
