export interface TrainingDto {
  id: string;
  title: string;
  description: string;
  category: string;
  maxEnrollment: number;
  startDate: Date | string;
  endDate: Date | string;
  locationName: string;
  status: string;
  enrollmentCount: number;
  availableSpots: number;
  createdAt: Date | string;
  createdByCoordinatorId: string;
}

export interface TrainingDetailDto {
  id: string;
  title: string;
  description: string;
  category: string;
  maxEnrollment: number;
  startDate: Date | string;
  endDate: Date | string;
  locationName: string;
  status: string;
  enrollmentCount: number;
  availableSpots: number;
  createdAt: Date | string;
}

export interface TrainingEnrollmentDto {
  id: string;
  trainingId: string;
  volunteerId: string;
  status: string;
  enrolledAt: Date | string;
  certificateNumber?: string | null;
  certificateIssuedAt?: Date | string | null;
}

export interface EnrollTrainingDto {
  volunteerId: string;
  status: string;
}

export interface TrainingFilterDto {
  category?: string;
  startDateFrom?: Date;
  startDateTo?: Date;
  availableSpotsOnly?: boolean;
  page: number;
  pageSize: number;
}
