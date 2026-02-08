import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  UploadDocumentRequestDto,
  UploadDocumentResponseDto,
  DocumentDto,
  VerifyDocumentDto
} from '../models/document.model';

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private apiUrl = `${environment.apiUrl}/documents`;

  constructor(private http: HttpClient) {}

  getUploadUrl(dto: UploadDocumentRequestDto): Observable<UploadDocumentResponseDto> {
    return this.http.post<UploadDocumentResponseDto>(`${this.apiUrl}/upload-url`, dto);
  }

  uploadDocument(uploadUrl: string, file: File): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': file.type });
    return this.http.put(uploadUrl, file, { headers });
  }

  getVolunteerDocuments(volunteerId: string): Observable<DocumentDto[]> {
    return this.http.get<DocumentDto[]>(`${this.apiUrl}/${volunteerId}`);
  }

  verifyDocument(documentId: string, dto: VerifyDocumentDto): Observable<DocumentDto> {
    return this.http.patch<DocumentDto>(`${this.apiUrl}/${documentId}/verify`, dto);
  }
}
