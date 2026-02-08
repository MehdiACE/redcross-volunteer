import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { VolunteerCommunicationsComponent } from './volunteer-communications.component';

describe('VolunteerCommunicationsComponent', () => {
  let component: VolunteerCommunicationsComponent;
  let fixture: ComponentFixture<B1jCommsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [B1jCommsComponent, ReactiveFormsModule, TranslateModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(B1jCommsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render composer form and status table', () => {
    const element = fixture.nativeElement as HTMLElement;
    const form = element.querySelector('[data-testid="comms-composer-form"]');
    const table = element.querySelector('[data-testid="comms-status-table"]');

    expect(form).toBeTruthy();
    expect(table).toBeTruthy();
  });

  it('should render status rows when messages are present', () => {
    component.messages = [
      { recipient: 'Guardian A', channel: 'Email', status: 'Sent', sentAt: '2026-01-01' }
    ];
    fixture.detectChanges();

    const rows = fixture.nativeElement.querySelectorAll('[data-testid="comms-status-row"]');
    expect(rows.length).toBe(1);
  });
});
