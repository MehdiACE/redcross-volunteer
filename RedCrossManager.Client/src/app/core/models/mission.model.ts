export interface MissionDto {
  id: string;
  title: string;
  description: string;
  missionType: string;
  location: string;
  startAt: Date;
  endAt: Date;
  requiredCertifications: string[];
  volunteersNeeded: number;
  travelBufferMinutes: number;
  published: boolean;
  createdAt: Date;
  createdBy: string;
  availableSlots: number;
}

export interface ApplyMissionDto {
  volunteerId: string;
}

export interface AssignVolunteersDto {
  volunteerIds: string[];
  roleDescription?: string;
}

export interface AssignmentDto {
  id: string;
  missionId: string;
  volunteerId: string;
  status: string;
  roleDescription?: string;
  assignedAt: Date;
  reminderSentAt?: Date | null;
}
