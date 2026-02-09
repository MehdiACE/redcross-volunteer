import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateFakeLoader, TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { of } from 'rxjs';
import { VolunteerCommunicationsComponent } from './volunteer-communications.component';
import { CommunicationService } from '../../core/services/communication.service';

describe('VolunteerCommunicationsComponent', () => {
  let component: VolunteerCommunicationsComponent;
  let fixture: ComponentFixture<VolunteerCommunicationsComponent>;

  beforeEach(async () => {
    const communicationServiceSpy = jasmine.createSpyObj('CommunicationService', [
      'sendCommunication',
    ]);

    await TestBed.configureTestingModule({
      imports: [
        VolunteerCommunicationsComponent,
        ReactiveFormsModule,
        TranslateModule.forRoot({
          loader: { provide: TranslateLoader, useClass: TranslateFakeLoader },
        }),
      ],
      providers: [{ provide: CommunicationService, useValue: communicationServiceSpy }],
    }).compileComponents();

    const communicationService = TestBed.inject(
      CommunicationService,
    ) as jasmine.SpyObj<CommunicationService>;
    communicationService.sendCommunication.and.returnValue(of({ totalRecipients: 0 } as any));

    fixture = TestBed.createComponent(VolunteerCommunicationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render composer form', () => {
    const element = fixture.nativeElement as HTMLElement;
    const form = element.querySelector('form');

    expect(form).toBeTruthy();
    expect(component.composerForm).toBeTruthy();
  });
});
