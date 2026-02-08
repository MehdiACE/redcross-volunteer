export interface RegisterVolunteerDto {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phone: string;
  dateOfBirth: Date;
  addressStreet: string;
  addressCity: string;
  addressStateProvince: string;
  addressPostalCode: string;
  addressCountry: string;
  emergencyContactName: string;
  emergencyContactPhone: string;
  areasOfInterest: string[];
  availability: {
    daysOfWeek: string[];
    timePreference: string;
  };
  languagePreference: string;
}

export interface VolunteerDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  dateOfBirth: Date;
  status: string;
  languagePreference: string;
  registeredAt: Date;
  isMinor: boolean;
  smsOptIn: boolean;
}

export interface SmsOptInDto {
  smsOptIn: boolean;
}

export interface UpdateStatusDto {
  status: string;
}

export interface LoginResponseDto {
  userId: string;
  accessToken: string;
  expiresAtUtc: Date;
  roles: string[];
}
