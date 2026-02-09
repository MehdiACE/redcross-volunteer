import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DocumentsComponent } from './documents.component';
import { DocumentService } from '../../core/services/document.service';
import { AuthService } from '../../core/services/auth.service';
import { TranslateModule } from '@ngx-translate/core';
import { of, throwError } from 'rxjs';
import { DocumentDto } from '../../core/models/document.model';

describe('DocumentsComponent', () => {
  let component: DocumentsComponent;
  let fixture: ComponentFixture<DocumentsComponent>;
  let documentService: jasmine.SpyObj<DocumentService>;
  let authService: jasmine.SpyObj<AuthService>;

  const mockDocs: DocumentDto[] = [
    {
      id: 'doc-1',
      volunteerId: 'vol-1',
      category: 'Identification',
      fileName: 'id.pdf',
      fileUrl: 'http://localhost/uploads/doc-1/id.pdf',
      contentType: 'application/pdf',
      sizeBytes: 1024,
      uploadedAt: new Date(),
      expiresAt: null,
      verificationStatus: 'Pending',
      virusScanStatus: 'Clean',
      reviewerNotes: null,
    },
  ];

  beforeEach(async () => {
    const documentSpy = jasmine.createSpyObj('DocumentService', [
      'getVolunteerDocuments',
      'getUploadUrl',
      'uploadDocument',
    ]);
    const authSpy = jasmine.createSpyObj('AuthService', ['getUserId']);

    await TestBed.configureTestingModule({
      imports: [DocumentsComponent, TranslateModule.forRoot()],
      providers: [
        { provide: DocumentService, useValue: documentSpy },
        { provide: AuthService, useValue: authSpy },
      ],
    }).compileComponents();

    documentService = TestBed.inject(DocumentService) as jasmine.SpyObj<DocumentService>;
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;

    fixture = TestBed.createComponent(DocumentsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load documents on init', () => {
    authService.getUserId.and.returnValue('vol-1');
    documentService.getVolunteerDocuments.and.returnValue(of(mockDocs));

    component.ngOnInit();

    expect(documentService.getVolunteerDocuments).toHaveBeenCalledWith('vol-1');
    expect(component.documents.length).toBe(1);
  });

  it('should handle load error', () => {
    authService.getUserId.and.returnValue('vol-1');
    documentService.getVolunteerDocuments.and.returnValue(throwError(() => new Error('fail')));

    component.ngOnInit();

    expect(component.isLoading).toBeFalse();
  });
});
