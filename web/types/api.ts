export type Category = "Barber" | "Stylist" | "Dentist" | "Psychologist";

export type AppointmentStatus = "Scheduled" | "Completed" | "Cancelled" | "NoShow";

export type DayOfWeek =
  | "Sunday"
  | "Monday"
  | "Tuesday"
  | "Wednesday"
  | "Thursday"
  | "Friday"
  | "Saturday";

export interface WorkingScheduleRuleRequest {
  days: DayOfWeek[];
  startTime: string;
  endTime: string;
  lunchBreakStartTime?: string | null;
  lunchBreakEndTime?: string | null;
}

export interface RegisterProfessionalRequest {
  fullName: string;
  email: string;
  phoneNumber: string;
  slug: string;
  category: Category;
  appointmentDurationMinutes: number;
  workingSchedule: WorkingScheduleRuleRequest[];
  password: string;
  inviteToken?: string;
}

export interface LoginProfessionalRequest {
  email: string;
  password: string;
}

export interface RegisterBusinessRequest {
  name: string;
  email: string;
  slug: string;
  password: string;
}

export interface LoginBusinessRequest {
  email: string;
  password: string;
}

export interface RosterProfessionalSummary {
  fullName: string;
  slug: string;
  category: Category;
}

export interface BusinessProfessionalSummary {
  fullName: string;
  slug: string;
  category: Category;
  coverPhotoUrls: string[];
}

export interface BusinessPublicProfile {
  name: string;
  slug: string;
  coverImageUrl: string | null;
  spacePhotos: string[];
  professionals: BusinessProfessionalSummary[];
}

export interface BusinessInviteInfo {
  businessName: string;
}

export interface BookAppointmentRequest {
  customerName: string;
  customerPhone: string;
  date: string;
  startTime: string;
}

export interface PortfolioPhotoPublicSummary {
  imageUrl: string;
  caption: string | null;
}

export interface ProfessionalPublicProfile {
  fullName: string;
  slug: string;
  category: Category;
  appointmentDurationMinutes: number;
  photos: PortfolioPhotoPublicSummary[];
}

export type PortfolioPhotoStatus = "Pending" | "Approved" | "Rejected";

export interface PortfolioPhoto {
  id: string;
  imageUrl: string;
  caption: string | null;
  status: PortfolioPhotoStatus;
  createdAt: string;
}

export interface PortfolioUploadSignature {
  signature: string;
  timestamp: number;
  apiKey: string;
  cloudName: string;
  folder: string;
  transformation: string;
}

export interface AdminPendingPhoto {
  id: string;
  imageUrl: string;
  caption: string | null;
  createdAt: string;
  professionalId: string;
  professionalFullName: string;
  professionalSlug: string;
  professionalCategory: Category;
  businessName: string | null;
  businessSlug: string | null;
}

export type BusinessPhotoKind = "Cover" | "Space";

export interface BusinessPhoto {
  id: string;
  kind: BusinessPhotoKind;
  imageUrl: string;
  caption: string | null;
  status: PortfolioPhotoStatus;
  createdAt: string;
}

export interface AdminPendingBusinessPhoto {
  id: string;
  kind: BusinessPhotoKind;
  imageUrl: string;
  caption: string | null;
  createdAt: string;
  businessId: string;
  businessName: string;
  businessSlug: string;
}

export interface AvailabilityResult {
  availableSlots: string[];
  isWorkingDay: boolean;
  message: string | null;
}

export interface ScheduleExceptionRequest {
  startDate: string;
  endDate: string;
  startTime?: string;
  endTime?: string;
  lunchBreakStartTime?: string;
  lunchBreakEndTime?: string;
  message?: string;
}

export interface ScheduleException {
  id: string;
  startDate: string;
  endDate: string;
  startTime: string | null;
  endTime: string | null;
  lunchBreakStartTime: string | null;
  lunchBreakEndTime: string | null;
  message: string | null;
}

export interface AppointmentSummary {
  id: string;
  customerName: string;
  customerPhone: string;
  date: string;
  startTime: string;
  endTime: string;
  status: AppointmentStatus;
}

export interface AppointmentsPageResponse {
  appointments: AppointmentSummary[];
  hasMore: boolean;
}

export type StatsRange = "Day" | "Week" | "Month";

export interface AppointmentStats {
  completedCount: number;
  noShowCount: number;
  cancelledCount: number;
  distinctCustomersCount: number;
  from: string;
  to: string;
}

export type AuthSession =
  | {
      accountType: "professional";
      token: string;
      professionalId: string;
      slug: string;
    }
  | {
      accountType: "business";
      token: string;
      businessId: string;
      slug: string;
      name: string;
    };

export interface ApiProblemDetails {
  status?: number;
  title?: string;
  detail?: string;
}
