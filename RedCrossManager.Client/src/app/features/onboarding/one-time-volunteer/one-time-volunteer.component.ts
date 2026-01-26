import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PDFDocument, StandardFonts, rgb } from 'pdf-lib';

@Component({
  selector: 'app-one-time-volunteer',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSnackBarModule,
    TranslateModule
  ],
  templateUrl: './one-time-volunteer.component.html'
})
export class OneTimeVolunteerComponent implements OnInit {
  form!: FormGroup;
  isGenerating = false;

  constructor(
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      fullName: ['', [Validators.required, Validators.maxLength(200)]],
      birthDate: ['', Validators.required],
      birthPlace: ['', [Validators.required, Validators.maxLength(120)]],
      phones: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(255)]],
      address: ['', [Validators.required, Validators.maxLength(300)]],
      action: ['', [Validators.required, Validators.maxLength(200)]],
      structure: ['', [Validators.required, Validators.maxLength(200)]],
      emergencyContactName: ['', [Validators.required, Validators.maxLength(200)]],
      emergencyContactRelation: ['', [Validators.required, Validators.maxLength(100)]],
      emergencyContactAddress: ['', [Validators.required, Validators.maxLength(300)]],
      emergencyContactPhones: ['', [Validators.required, Validators.maxLength(100)]],
      acceptsPrinciples: [false, Validators.requiredTrue],
      acceptsImage: [false],
      keepDataConsent: ['yes', Validators.required],
      signedAt: ['', [Validators.required, Validators.maxLength(120)]],
      signedOn: ['', Validators.required],
      signature: ['', [Validators.required, Validators.maxLength(120)]]
    });
  }

  async generatePdf(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.snackBar.open(
        this.translate.instant('oneTimeVolunteer.errors.invalidForm'),
        this.translate.instant('common.close'),
        { duration: 4000 }
      );
      return;
    }

    this.isGenerating = true;
    try {
      const templateUrl = '/assets/forms/formulaire-benevole-ponctuel.pdf';
      const existingPdfBytes = await fetch(templateUrl).then(res => res.arrayBuffer());
      const pdfDoc = await PDFDocument.load(existingPdfBytes);
      const font = await pdfDoc.embedFont(StandardFonts.Helvetica);

      const page = pdfDoc.getPages()[0];
      const { width, height } = page.getSize();

      const v = this.form.value;
      const fontSize = 10;
      const textColor = rgb(0, 0, 0);

      const drawText = (text: string, x: number, y: number) => {
        page.drawText(text ?? '', { x, y, size: fontSize, font, color: textColor });
      };

      const drawMultiline = (text: string, x: number, y: number, lineHeight = 12) => {
        const lines = this.wrapText(text ?? '', 60);
        lines.forEach((line, i) => drawText(line, x, y - i * lineHeight));
      };

      // Coordinates tuned for A4 (595x842). Adjust as needed.
      drawText(v.fullName, 110, 602);
      drawText(this.formatDate(v.birthDate), 85, 583);
      drawText(v.birthPlace, 155, 583);
      drawText(v.phones, 60, 564);
      drawText(v.email, 60, 545);
      drawMultiline(v.address, 80, 526, 12);
      drawText(v.action, 140, 503);
      drawText(v.structure, 200, 485);

      drawText(v.emergencyContactName, 170, 445);
      drawText(v.emergencyContactRelation, 120, 427);
      drawMultiline(v.emergencyContactAddress, 90, 408, 12);
      drawText(v.emergencyContactPhones, 60, 386);

      if (v.acceptsPrinciples) drawText('X', 36, 358);
      if (v.acceptsImage) drawText('X', 36, 338);

      if (v.keepDataConsent === 'yes') {
        drawText('X', 60, 258);
      } else {
        drawText('X', 150, 258);
      }

      drawText(v.signedAt, 70, 236);
      drawText(this.formatDate(v.signedOn), 140, 236);
      drawText(v.signature, 250, 236);

      const pdfBytes = await pdfDoc.save();
      this.downloadPdf(pdfBytes, 'formulaire-benevole-ponctuel-rempli.pdf');

      this.snackBar.open(
        this.translate.instant('oneTimeVolunteer.messages.generated'),
        this.translate.instant('common.close'),
        { duration: 4000 }
      );
    } catch (error) {
      this.snackBar.open(
        this.translate.instant('oneTimeVolunteer.errors.generationFailed'),
        this.translate.instant('common.close'),
        { duration: 5000 }
      );
    } finally {
      this.isGenerating = false;
    }
  }

  private wrapText(text: string, maxChars: number): string[] {
    if (!text) return [''];
    const words = text.split(' ');
    const lines: string[] = [];
    let current = '';
    for (const word of words) {
      const next = current ? `${current} ${word}` : word;
      if (next.length > maxChars) {
        lines.push(current);
        current = word;
      } else {
        current = next;
      }
    }
    if (current) lines.push(current);
    return lines;
  }

  private formatDate(value: Date): string {
    if (!value) return '';
    const d = new Date(value);
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    return `${day}/${month}/${year}`;
  }

  private downloadPdf(bytes: Uint8Array, filename: string): void {
    const blob = new Blob([bytes], { type: 'application/pdf' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    window.URL.revokeObjectURL(url);
  }
}
