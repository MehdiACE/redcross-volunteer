import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import {
  CommunicationService,
  SendCommunicationRequest,
} from '../../core/services/communication.service';

interface MessageTemplate {
  fr: { subject: string; body: string };
  en: { subject: string; body: string };
}

@Component({
  selector: 'app-b1j-comms',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatButtonModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './b1j-comms.component.html',
  styleUrls: ['./b1j-comms.component.scss'],
})
export class B1jCommsComponent implements OnInit {
  composerForm!: FormGroup;
  isLoading = false;

  segments = [
    { value: 'B1J - Missing Consent', labelKey: 'b1jComms.segments.missingConsent' },
    { value: 'B1J - In Onboarding', labelKey: 'b1jComms.segments.inOnboarding' },
    { value: 'B1J - Assigned', labelKey: 'b1jComms.segments.assigned' },
    { value: 'Active Volunteers', labelKey: 'b1jComms.segments.activeVolunteers' },
  ];

  templates: Record<string, MessageTemplate> = {
    'B1J - Missing Consent': {
      fr: {
        subject: 'Consentement parental requis',
        body: "Bonjour {FirstName},\n\nNous avons besoin du consentement de votre parent/tuteur pour compléter votre inscription au programme de bénévolat de la Croix-Rouge.\n\nVeuillez demander à votre parent/tuteur de cliquer sur ce lien pour remplir le formulaire de consentement :\n{ConsentLink}\n\nSi vous avez des questions, n'hésitez pas à nous contacter.\n\nMerci,\nÉquipe Croix-Rouge Canada",
      },
      en: {
        subject: 'Parental Consent Required',
        body: "Hello {FirstName},\n\nWe need your parent/guardian consent to complete your registration for the Red Cross volunteer program.\n\nPlease ask your parent/guardian to click this link to fill out the consent form:\n{ConsentLink}\n\nIf you have any questions, please don't hesitate to contact us.\n\nThank you,\nCanadian Red Cross Team",
      },
    },
    'B1J - In Onboarding': {
      fr: {
        subject: 'Progression de votre inscription',
        body: 'Bonjour {FirstName},\n\nNous avons hâte de vous accueillir dans notre équipe de bénévoles !\n\nVoici les prochaines étapes pour compléter votre inscription :\n- Compléter les formations requises\n- Soumettre les documents nécessaires\n\nConnectez-vous à votre tableau de bord pour voir votre progression.\n\nMerci,\nÉquipe Croix-Rouge Canada',
      },
      en: {
        subject: 'Your Registration Progress',
        body: "Hello {FirstName},\n\nWe're excited to welcome you to our volunteer team!\n\nHere are the next steps to complete your registration:\n- Complete required trainings\n- Submit necessary documents\n\nLog in to your dashboard to see your progress.\n\nThank you,\nCanadian Red Cross Team",
      },
    },
    'B1J - Assigned': {
      fr: {
        subject: 'Nouvelle mission assignée',
        body: 'Bonjour {FirstName},\n\nUne nouvelle mission vous a été assignée !\n\nConnectez-vous à votre tableau de bord pour voir les détails et confirmer votre disponibilité.\n\nMerci de votre engagement,\nÉquipe Croix-Rouge Canada',
      },
      en: {
        subject: 'New Mission Assigned',
        body: 'Hello {FirstName},\n\nA new mission has been assigned to you!\n\nLog in to your dashboard to see details and confirm your availability.\n\nThank you for your commitment,\nCanadian Red Cross Team',
      },
    },
    'Active Volunteers': {
      fr: {
        subject: 'Message aux bénévoles actifs',
        body: 'Bonjour {FirstName},\n\n[Votre message ici]\n\nMerci de votre engagement continu,\nÉquipe Croix-Rouge Canada',
      },
      en: {
        subject: 'Message to Active Volunteers',
        body: 'Hello {FirstName},\n\n[Your message here]\n\nThank you for your continued commitment,\nCanadian Red Cross Team',
      },
    },
  };

  constructor(
    private fb: FormBuilder,
    private communicationService: CommunicationService,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
  ) {}

  ngOnInit(): void {
    const currentLang = this.translate.currentLang || 'fr';

    this.composerForm = this.fb.group({
      segment: ['', Validators.required],
      language: [currentLang, Validators.required],
      sendEmail: [true],
      sendSms: [false],
      subject: ['', Validators.required],
      bodyTemplate: ['', Validators.required],
    });

    // Load template when segment or language changes
    this.composerForm.get('segment')?.valueChanges.subscribe(() => this.loadTemplate());
    this.composerForm.get('language')?.valueChanges.subscribe(() => this.loadTemplate());

    // At least one channel must be selected
    this.composerForm.get('sendEmail')?.valueChanges.subscribe((email) => {
      if (!email && !this.composerForm.get('sendSms')?.value) {
        this.composerForm.get('sendSms')?.setValue(true);
      }
    });

    this.composerForm.get('sendSms')?.valueChanges.subscribe((sms) => {
      if (!sms && !this.composerForm.get('sendEmail')?.value) {
        this.composerForm.get('sendEmail')?.setValue(true);
      }
    });
  }

  loadTemplate(): void {
    const segment = this.composerForm.get('segment')?.value;
    const language = this.composerForm.get('language')?.value || 'fr';

    if (segment && this.templates[segment]) {
      const template = this.templates[segment][language as 'fr' | 'en'];
      this.composerForm.patchValue(
        {
          subject: template.subject,
          bodyTemplate: template.body,
        },
        { emitEvent: false },
      );
    }
  }

  submit(): void {
    if (this.composerForm.invalid) {
      this.composerForm.markAllAsTouched();
      return;
    }

    const formValue = this.composerForm.value;

    // Calculate channels bitmask
    let channels = 0;
    if (formValue.sendEmail) channels |= 1; // Email = 1
    if (formValue.sendSms) channels |= 2; // SMS = 2

    const request: SendCommunicationRequest = {
      segment: formValue.segment,
      channels: channels,
      language: formValue.language,
      subject: formValue.subject,
      bodyTemplate: formValue.bodyTemplate,
      recipientVolunteerIds: null,
    };

    this.isLoading = true;
    this.communicationService.sendCommunication(request).subscribe({
      next: (result) => {
        this.isLoading = false;
        this.snackBar.open(
          this.translate.instant('b1jComms.success', { count: result.totalRecipients }),
          this.translate.instant('common.close'),
          { duration: 5000 },
        );
        // Reset form but keep language
        this.composerForm.reset({
          language: formValue.language,
          sendEmail: true,
          sendSms: false,
        });
      },
      error: (error) => {
        this.isLoading = false;
        this.snackBar.open(
          this.translate.instant('b1jComms.error'),
          this.translate.instant('common.close'),
          { duration: 5000, panelClass: ['error-snackbar'] },
        );
        console.error('Failed to send communication:', error);
      },
    });
  }

  getChannelLabel(): string {
    const email = this.composerForm.get('sendEmail')?.value;
    const sms = this.composerForm.get('sendSms')?.value;

    if (email && sms) return this.translate.instant('b1jComms.channels.both');
    if (email) return this.translate.instant('b1jComms.channels.email');
    if (sms) return this.translate.instant('b1jComms.channels.sms');
    return '';
  }
}
