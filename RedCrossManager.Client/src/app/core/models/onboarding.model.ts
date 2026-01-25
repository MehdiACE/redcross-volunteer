export interface OnboardingProgressDto {
  volunteerId: string;
  volunteer: {
    firstName: string;
    lastName: string;
    email: string;
    phone: string;
  };
  steps: OnboardingStepDto[];
  currentStatus: string;
  isMinor: boolean;
  parentalConsentApproved: boolean;
  startedAt: Date;
  completedAt?: Date;
}

export interface OnboardingStepDto {
  id: string;
  stepNumber: number;
  title: string;
  description: string;
  status: 'Pending' | 'Submitted' | 'Completed' | 'Rejected';
  submittedAt?: Date;
  reviewedAt?: Date;
  reviewerNotes?: string;
}

export interface SubmitStepDto {
  stepId: string;
}

export interface ReviewStepDto {
  approved: boolean;
  notes?: string;
}
