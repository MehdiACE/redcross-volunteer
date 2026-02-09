import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CommunicationHistoryComponent } from './communication-history.component';
import { CommunicationService } from '../../../core/services/communication.service';
import { of, throwError } from 'rxjs';
import { TranslateModule } from '@ngx-translate/core';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('CommunicationHistoryComponent', () => {
  let component: CommunicationHistoryComponent;
  let fixture: ComponentFixture<CommunicationHistoryComponent>;
  let mockCommunicationService: jasmine.SpyObj<CommunicationService>;

  beforeEach(async () => {
    mockCommunicationService = jasmine.createSpyObj('CommunicationService', [
      'getRecentCommunications',
    ]);

    await TestBed.configureTestingModule({
      imports: [CommunicationHistoryComponent, TranslateModule.forRoot(), NoopAnimationsModule],
      providers: [{ provide: CommunicationService, useValue: mockCommunicationService }],
    }).compileComponents();

    fixture = TestBed.createComponent(CommunicationHistoryComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load messages on init', () => {
    const mockMessages = [
      {
        id: '1',
        segment: 'B1J - Missing Consent',
        channels: 3,
        language: 'fr',
        subject: 'Test Subject',
        bodyTemplate: 'Test Body',
        sentAt: new Date().toISOString(),
        createdBy: 'Coordinator',
        totalRecipients: 16,
        queuedCount: 5,
        sentCount: 10,
        failedCount: 1,
        bouncedCount: 0,
      },
    ];

    mockCommunicationService.getRecentCommunications.and.returnValue(of(mockMessages));
    component.ngOnInit();

    expect(mockCommunicationService.getRecentCommunications).toHaveBeenCalledWith(50);
    expect(component.messages.length).toBe(1);
    expect(component.isLoading).toBeFalse();
  });

  it('should handle load error gracefully', () => {
    mockCommunicationService.getRecentCommunications.and.returnValue(
      throwError(() => new Error('Load failed')),
    );
    spyOn(console, 'error');

    component.loadHistory();

    expect(component.isLoading).toBeFalse();
    expect(console.error).toHaveBeenCalled();
  });

  it('should calculate channel label correctly', () => {
    expect(component.getChannelLabel(1)).toBe('Email');
    expect(component.getChannelLabel(2)).toBe('SMS');
    expect(component.getChannelLabel(3)).toBe('Email + SMS');
    expect(component.getChannelLabel(0)).toBe('None');
  });

  it('should calculate success rate correctly', () => {
    const message = {
      id: '1',
      segment: 'Test',
      channels: 1,
      language: 'en',
      subject: 'Test',
      bodyTemplate: 'Test',
      sentAt: new Date().toISOString(),
      createdBy: 'Coordinator',
      totalRecipients: 10,
      queuedCount: 0,
      sentCount: 8,
      failedCount: 2,
      bouncedCount: 0,
    };

    expect(component.getSuccessRate(message)).toBe(80);
  });
});
