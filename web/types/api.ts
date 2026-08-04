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
}

export interface LoginProfessionalRequest {
  email: string;
  password: string;
}

export interface BookAppointmentRequest {
  customerName: string;
  customerPhone: string;
  date: string;
  startTime: string;
}

export interface ProfessionalPublicProfile {
  fullName: string;
  slug: string;
  category: Category;
  appointmentDurationMinutes: number;
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

export interface AuthSession {
  token: string;
  professionalId: string;
  slug: string;
}

export interface ApiProblemDetails {
  status?: number;
  title?: string;
  detail?: string;
}
