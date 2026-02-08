export interface UploadDocumentRequestDto {
  category: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  expiresAt?: Date | null;
}

export interface UploadDocumentResponseDto {
  documentId: string;
  uploadUrl: string;
  fileUrl: string;
}

export interface DocumentDto {
  id: string;
  volunteerId: string;
  category: string;
  fileName: string;
  fileUrl: string;
  contentType: string;
  sizeBytes: number;
  uploadedAt: Date;
  expiresAt?: Date | null;
  verificationStatus: string;
  virusScanStatus: string;
  reviewerNotes?: string | null;
}

export interface VerifyDocumentDto {
  status: string;
  reviewerNotes?: string | null;
}
