import { VolunteerDto } from './volunteer.model';

export interface DashboardOnboardingSummary {
  completedCount: number;
  totalCount: number;
  currentStepNumber: number;
  currentStep: string;
  isComplete: boolean;
  isMinor: boolean;
  parentalConsentApproved: boolean;
}

export interface DashboardAssignment {
  id: string;
  title: string;
  startAt: string;
  endAt: string;
  location: string;
  status: string;
  roleDescription?: string;
}

export interface DashboardTraining {
  id: string;
  title: string;
  category: string;
  startAt: string;
  endAt: string;
  status: string;
  certificateUrl?: string;
}

export interface DashboardCertification {
  id: string;
  type: string;
  issuedAt: string;
  expiresAt: string;
  status: string;
}

export interface DashboardAlert {
  type: string;
  message: string;
  dueAt?: string;
  actionUrl?: string;
}

export interface VolunteerDashboardDto extends VolunteerDto {
  onboarding: DashboardOnboardingSummary;
  upcomingAssignments: DashboardAssignment[];
  trainings: DashboardTraining[];
  certifications: DashboardCertification[];
  alerts: DashboardAlert[];
}
