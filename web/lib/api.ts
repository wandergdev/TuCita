import type {
  AppointmentStatus,
  AppointmentStats,
  AppointmentSummary,
  AppointmentsPageResponse,
  ApiProblemDetails,
  AuthSession,
  AvailabilityResult,
  BookAppointmentRequest,
  LoginProfessionalRequest,
  ProfessionalPublicProfile,
  RegisterProfessionalRequest,
  ScheduleException,
  ScheduleExceptionRequest,
  StatsRange,
  WorkingScheduleRuleRequest,
} from "@/types/api";
import { clearSession } from "@/lib/auth";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5222";

export class ApiError extends Error {
  status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

interface RawId {
  value: string;
}

interface RawRegisterProfessionalResponse {
  professionalId: RawId;
}

interface RawLoginProfessionalResponse {
  token: string;
  professionalId: RawId;
  slug: string;
}

interface RawBookAppointmentResponse {
  appointmentId: RawId;
}

interface RawAppointmentSummary extends Omit<AppointmentSummary, "id"> {
  id: RawId;
}

interface RawGetProfessionalAppointmentsResponse {
  appointments: RawAppointmentSummary[];
  hasMore: boolean;
}

interface RawGetAvailabilityResponse {
  availableSlots: string[];
  isWorkingDay: boolean;
  message: string | null;
}

interface RawGetMyWorkingScheduleResponse {
  rules: WorkingScheduleRuleRequest[];
}

interface RawScheduleException extends Omit<ScheduleException, "id"> {
  id: RawId;
}

interface RawGetScheduleExceptionsResponse {
  exceptions: RawScheduleException[];
}

interface RawCreateScheduleExceptionResponse {
  scheduleExceptionId: RawId;
}

interface ApiFetchOptions {
  method?: "GET" | "POST" | "PUT" | "DELETE";
  body?: unknown;
  token?: string | null;
  cache?: RequestCache;
}

async function apiFetch<T>(path: string, options: ApiFetchOptions = {}): Promise<T> {
  const headers: Record<string, string> = {};
  let payload: string | undefined;

  if (options.body !== undefined) {
    headers["Content-Type"] = "application/json";
    payload = JSON.stringify(options.body);
  }

  if (options.token) {
    headers.Authorization = `Bearer ${options.token}`;
  }

  const response = await fetch(`${API_URL}${path}`, {
    method: options.method ?? "GET",
    headers,
    body: payload,
    cache: options.cache ?? "no-store",
  });

  if (response.status === 204) {
    return undefined as T;
  }

  const isJson = response.headers.get("content-type")?.includes("application/json") ?? false;
  const data = isJson ? await response.json() : undefined;

  if (!response.ok) {
    if (response.status === 401 && options.token) {
      clearSession();
      if (typeof window !== "undefined" && window.location.pathname !== "/login") {
        window.location.href = "/login";
      }
      throw new ApiError(401, "Tu sesión expiró.");
    }

    const problem = data as ApiProblemDetails | undefined;
    throw new ApiError(
      response.status,
      problem?.detail ?? problem?.title ?? "Ocurrió un error inesperado.",
    );
  }

  return data as T;
}

export async function registerProfessional(payload: RegisterProfessionalRequest): Promise<string> {
  const response = await apiFetch<RawRegisterProfessionalResponse>("/api/professionals/register", {
    method: "POST",
    body: payload,
  });
  return response.professionalId.value;
}

export async function loginProfessional(payload: LoginProfessionalRequest): Promise<AuthSession> {
  const response = await apiFetch<RawLoginProfessionalResponse>("/api/professionals/login", {
    method: "POST",
    body: payload,
  });
  return {
    token: response.token,
    professionalId: response.professionalId.value,
    slug: response.slug,
  };
}

export async function verifyEmail(token: string): Promise<void> {
  await apiFetch<void>("/api/professionals/verify-email", {
    method: "POST",
    body: { token },
  });
}

export async function resendVerificationEmail(email: string): Promise<void> {
  await apiFetch<void>("/api/professionals/resend-verification", {
    method: "POST",
    body: { email },
  });
}

export async function getProfessionalProfile(slug: string): Promise<ProfessionalPublicProfile | null> {
  try {
    return await apiFetch<ProfessionalPublicProfile>(`/api/professionals/${encodeURIComponent(slug)}`);
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) return null;
    throw error;
  }
}

export async function getAvailability(slug: string, date: string): Promise<AvailabilityResult> {
  const response = await apiFetch<RawGetAvailabilityResponse>(
    `/api/professionals/${encodeURIComponent(slug)}/availability?date=${date}`,
  );
  return {
    availableSlots: response.availableSlots,
    isWorkingDay: response.isWorkingDay,
    message: response.message,
  };
}

export async function getMyWorkingSchedule(token: string): Promise<WorkingScheduleRuleRequest[]> {
  const response = await apiFetch<RawGetMyWorkingScheduleResponse>("/api/professionals/me/schedule", {
    token,
  });
  return response.rules;
}

export async function updateWorkingSchedule(
  token: string,
  workingSchedule: WorkingScheduleRuleRequest[],
): Promise<void> {
  await apiFetch<void>("/api/professionals/me/schedule", {
    method: "PUT",
    body: { workingSchedule },
    token,
  });
}

export async function getScheduleExceptions(token: string): Promise<ScheduleException[]> {
  const response = await apiFetch<RawGetScheduleExceptionsResponse>(
    "/api/professionals/me/schedule/exceptions",
    { token },
  );
  return response.exceptions.map((exception) => ({ ...exception, id: exception.id.value }));
}

export async function createScheduleException(
  token: string,
  payload: ScheduleExceptionRequest,
): Promise<string> {
  const response = await apiFetch<RawCreateScheduleExceptionResponse>(
    "/api/professionals/me/schedule/exceptions",
    { method: "POST", body: payload, token },
  );
  return response.scheduleExceptionId.value;
}

export async function deleteScheduleException(token: string, id: string): Promise<void> {
  await apiFetch<void>(`/api/professionals/me/schedule/exceptions/${encodeURIComponent(id)}`, {
    method: "DELETE",
    token,
  });
}

export async function bookAppointment(
  slug: string,
  payload: BookAppointmentRequest,
): Promise<string> {
  const response = await apiFetch<RawBookAppointmentResponse>(
    `/api/professionals/${encodeURIComponent(slug)}/appointments`,
    { method: "POST", body: payload },
  );
  return response.appointmentId.value;
}

export async function checkExistingAppointment(
  slug: string,
  customerPhone: string,
  date: string,
): Promise<boolean> {
  const response = await apiFetch<{ hasExistingAppointment: boolean }>(
    `/api/professionals/${encodeURIComponent(slug)}/appointments/existing` +
      `?customerPhone=${encodeURIComponent(customerPhone)}&date=${date}`,
  );
  return response.hasExistingAppointment;
}

export async function getMyAppointments(
  token: string,
  options: { status?: AppointmentStatus; skip?: number; take?: number } = {},
): Promise<AppointmentsPageResponse> {
  const params = new URLSearchParams();
  if (options.status) params.set("status", options.status);
  if (options.skip !== undefined) params.set("skip", String(options.skip));
  if (options.take !== undefined) params.set("take", String(options.take));
  const query = params.toString();

  const response = await apiFetch<RawGetProfessionalAppointmentsResponse>(
    `/api/appointments/mine${query ? `?${query}` : ""}`,
    { token },
  );

  return {
    appointments: response.appointments.map((appointment) => ({
      ...appointment,
      id: appointment.id.value,
    })),
    hasMore: response.hasMore,
  };
}

export async function getAppointmentStats(
  token: string,
  range: StatsRange,
): Promise<AppointmentStats> {
  return apiFetch<AppointmentStats>(`/api/appointments/stats?range=${range}`, { token });
}

export async function completeAppointment(token: string, id: string): Promise<void> {
  await apiFetch<void>(`/api/appointments/${encodeURIComponent(id)}/complete`, {
    method: "POST",
    token,
  });
}

export async function cancelAppointment(token: string, id: string): Promise<void> {
  await apiFetch<void>(`/api/appointments/${encodeURIComponent(id)}/cancel`, {
    method: "POST",
    token,
  });
}

export async function markAppointmentAsNoShow(token: string, id: string): Promise<void> {
  await apiFetch<void>(`/api/appointments/${encodeURIComponent(id)}/no-show`, {
    method: "POST",
    token,
  });
}
