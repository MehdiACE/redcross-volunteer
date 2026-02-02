export interface AdminVolunteerListItem {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  status: string;
  languagePreference: string;
  registeredAt: string;
  isMinor: boolean;
  smsOptIn: boolean;
}

export interface AdminOnboardingStep {
  id: string;
  volunteerId: string;
  volunteerName: string;
  volunteerEmail: string;
  stepType: string;
  status: string;
  submittedAt?: string | null;
}

export interface ReviewStepRequest {
  approved: boolean;
  reviewerNotes?: string | null;
}
