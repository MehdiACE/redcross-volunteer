import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DocumentService } from '../../core/services/document.service';
import { AuthService } from '../../core/services/auth.service';
import { DocumentDto } from '../../core/models/document.model';

@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatIconModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    TranslateModule
  ],
  templateUrl: './documents.component.html',
  styleUrls: ['./documents.component.scss']
})
export class DocumentsComponent implements OnInit {
  documents: DocumentDto[] = [];
  displayedColumns: string[] = ['fileName', 'category', 'uploadedAt', 'status', 'actions'];
  isLoading = false;
  uploadInProgress = false;

  categories = [
    'Identification',
    'BackgroundCheck',
    'Certification',
    'MedicalForm',
    'ConsentForm'
  ];

  selectedCategory = '';
  selectedFile: File | null = null;

  constructor(
    private documentService: DocumentService,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadDocuments();
  }

  loadDocuments(): void {
    const volunteerId = this.authService.getUserId();
    if (!volunteerId) {
      return;
    }

    this.isLoading = true;
    this.documentService.getVolunteerDocuments(volunteerId).subscribe({
      next: (docs) => {
        this.documents = docs ?? [];
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.snackBar.open(this.translate.instant('documents.loadError'), undefined, { duration: 3000 });
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      this.selectedFile = null;
      return;
    }

    this.selectedFile = input.files[0];
  }

  uploadDocument(): void {
    if (!this.selectedFile || !this.selectedCategory) {
      this.snackBar.open(this.translate.instant('documents.selectFile'), undefined, { duration: 3000 });
      return;
    }

    this.uploadInProgress = true;
    const file = this.selectedFile;

    this.documentService.getUploadUrl({
      category: this.selectedCategory,
      fileName: file.name,
      contentType: file.type,
      sizeBytes: file.size
    }).subscribe({
      next: (response) => {
        this.documentService.uploadDocument(response.uploadUrl, file).subscribe({
          next: () => {
            this.uploadInProgress = false;
            this.selectedFile = null;
            this.selectedCategory = '';
            this.snackBar.open(this.translate.instant('documents.uploadSuccess'), undefined, { duration: 3000 });
            this.loadDocuments();
          },
          error: () => {
            this.uploadInProgress = false;
            this.snackBar.open(this.translate.instant('documents.uploadError'), undefined, { duration: 3000 });
          }
        });
      },
      error: () => {
        this.uploadInProgress = false;
        this.snackBar.open(this.translate.instant('documents.uploadError'), undefined, { duration: 3000 });
      }
    });
  }

  downloadDocument(document: DocumentDto): void {
    window.open(document.fileUrl, '_blank');
  }
}
