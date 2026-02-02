import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

interface CommsStatusRow {
  recipient: string;
  channel: string;
  status: string;
  sentAt: string;
}

@Component({
  selector: 'app-b1j-comms',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './b1j-comms.component.html'
})
export class B1jCommsComponent implements OnInit {
  composerForm!: FormGroup;
  messages: CommsStatusRow[] = [];

  segments = [
    { value: 'missing-consent', labelKey: 'b1jComms.segments.missingConsent' },
    { value: 'in-onboarding', labelKey: 'b1jComms.segments.inOnboarding' },
    { value: 'assigned', labelKey: 'b1jComms.segments.assigned' }
  ];

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.composerForm = this.fb.group({
      segment: ['', Validators.required],
      subject: ['', Validators.required],
      message: ['', Validators.required],
      sendEmail: [true],
      sendSms: [false]
    });
  }

  submit(): void {
    if (this.composerForm.invalid) {
      this.composerForm.markAllAsTouched();
      return;
    }

    // Placeholder for sending logic
  }
}
