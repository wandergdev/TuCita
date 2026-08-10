import type {
  AdminPendingBusinessPhoto,
  AdminPendingPhoto,
  AppointmentStatus,
  AppointmentStats,
  AppointmentSummary,
  AppointmentsPageResponse,
  ApiProblemDetails,
  AuthSession,
  AvailabilityResult,
  BookAppointmentRequest,
  BusinessInviteInfo,
  BusinessPhoto,
  BusinessPhotoKind,
  BusinessPublicProfile,
  LoginBusinessRequest,
  LoginProfessionalRequest,
  PortfolioPhoto,
  PortfolioUploadSignature,
  ProfessionalPublicProfile,
  RegisterBusinessRequest,
  RegisterProfessionalRequest,
  RosterProfessionalSummary,
  ScheduleException,
  ScheduleExceptionRequest,
  StatsRange,
  WorkingScheduleRuleRequest,
} from "@/types/api";
import { clearSession } from "@/lib/auth";
import { clearAdminSession } from "@/lib/adminAuth";

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

interface RawRegisterBusinessResponse {
  businessId: RawId;
}

interface RawLoginBusinessResponse {
  token: string;
  businessId: RawId;
  slug: string;
  name: string;
}

interface RawCreateBusinessInviteResponse {
  inviteUrl: string;
}

interface RawGetMyRosterResponse {
  professionals: RosterProfessionalSummary[];
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

interface RawPortfolioPhoto extends Omit<PortfolioPhoto, "id"> {
  id: RawId;
}

interface RawGetMyPortfolioResponse {
  photos: RawPortfolioPhoto[];
}

interface RawAddPortfolioPhotoResponse {
  portfolioPhotoId: RawId;
}

interface RawAdminPendingPhoto extends Omit<AdminPendingPhoto, "id" | "professionalId"> {
  id: RawId;
  professionalId: RawId;
}

interface RawGetAdminPendingPhotosResponse {
  photos: RawAdminPendingPhoto[];
}

interface RawAdminLoginResponse {
  token: string;
}

interface RawBusinessPhoto extends Omit<BusinessPhoto, "id"> {
  id: RawId;
}

interface RawGetMyBusinessPhotosResponse {
  photos: RawBusinessPhoto[];
}

interface RawAddBusinessPhotoResponse {
  businessPhotoId: RawId;
}

interface RawAdminPendingBusinessPhoto extends Omit<AdminPendingBusinessPhoto, "id" | "businessId"> {
  id: RawId;
  businessId: RawId;
}

interface RawGetAdminPendingBusinessPhotosResponse {
  photos: RawAdminPendingBusinessPhoto[];
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
      const isAdminRoute = typeof window !== "undefined" && window.location.pathname.startsWith("/admin");
      const loginPath = isAdminRoute ? "/admin/login" : "/login";

      if (isAdminRoute) {
        clearAdminSession();
      } else {
        clearSession();
      }

      if (typeof window !== "undefined" && window.location.pathname !== loginPath) {
        window.location.href = loginPath;
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
    accountType: "professional",
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

export async function forgotPassword(email: string): Promise<void> {
  await apiFetch<void>("/api/professionals/forgot-password", {
    method: "POST",
    body: { email },
  });
}

export async function resetPassword(token: string, newPassword: string): Promise<void> {
  await apiFetch<void>("/api/professionals/reset-password", {
    method: "POST",
    body: { token, newPassword },
  });
}

export async function forgotBusinessPassword(email: string): Promise<void> {
  await apiFetch<void>("/api/businesses/forgot-password", {
    method: "POST",
    body: { email },
  });
}

export async function resetBusinessPassword(token: string, newPassword: string): Promise<void> {
  await apiFetch<void>("/api/businesses/reset-password", {
    method: "POST",
    body: { token, newPassword },
  });
}

export async function changePassword(
  token: string,
  currentPassword: string,
  newPassword: string,
): Promise<void> {
  await apiFetch<void>("/api/professionals/me/change-password", {
    method: "POST",
    body: { currentPassword, newPassword },
    token,
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

export async function registerBusiness(payload: RegisterBusinessRequest): Promise<string> {
  const response = await apiFetch<RawRegisterBusinessResponse>("/api/businesses/register", {
    method: "POST",
    body: payload,
  });
  return response.businessId.value;
}

export async function loginBusiness(payload: LoginBusinessRequest): Promise<AuthSession> {
  const response = await apiFetch<RawLoginBusinessResponse>("/api/businesses/login", {
    method: "POST",
    body: payload,
  });
  return {
    accountType: "business",
    token: response.token,
    businessId: response.businessId.value,
    slug: response.slug,
    name: response.name,
  };
}

export async function verifyBusinessEmail(token: string): Promise<void> {
  await apiFetch<void>("/api/businesses/verify-email", {
    method: "POST",
    body: { token },
  });
}

export async function resendBusinessVerificationEmail(email: string): Promise<void> {
  await apiFetch<void>("/api/businesses/resend-verification", {
    method: "POST",
    body: { email },
  });
}

export async function getBusinessProfile(slug: string): Promise<BusinessPublicProfile | null> {
  try {
    return await apiFetch<BusinessPublicProfile>(`/api/businesses/${encodeURIComponent(slug)}`);
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) return null;
    throw error;
  }
}

export async function getBusinessInvite(token: string): Promise<BusinessInviteInfo> {
  return apiFetch<BusinessInviteInfo>(`/api/businesses/invites/${encodeURIComponent(token)}`);
}

export async function createBusinessInvite(token: string): Promise<string> {
  const response = await apiFetch<RawCreateBusinessInviteResponse>("/api/businesses/me/invites", {
    method: "POST",
    token,
  });
  return response.inviteUrl;
}

export async function getMyRoster(token: string): Promise<RosterProfessionalSummary[]> {
  const response = await apiFetch<RawGetMyRosterResponse>("/api/businesses/me/roster", { token });
  return response.professionals;
}

export async function getBusinessPhotoUploadSignature(token: string): Promise<PortfolioUploadSignature> {
  return apiFetch<PortfolioUploadSignature>("/api/businesses/me/photos/upload-signature", {
    method: "POST",
    token,
  });
}

export async function addBusinessPhoto(
  token: string,
  kind: BusinessPhotoKind,
  imageUrl: string,
  publicId: string,
  caption?: string,
): Promise<string> {
  const response = await apiFetch<RawAddBusinessPhotoResponse>("/api/businesses/me/photos", {
    method: "POST",
    body: { kind, imageUrl, publicId, caption },
    token,
  });
  return response.businessPhotoId.value;
}

export async function getMyBusinessPhotos(token: string): Promise<BusinessPhoto[]> {
  const response = await apiFetch<RawGetMyBusinessPhotosResponse>("/api/businesses/me/photos", { token });
  return response.photos.map((photo) => ({ ...photo, id: photo.id.value }));
}

export async function deleteBusinessPhoto(token: string, id: string): Promise<void> {
  await apiFetch<void>(`/api/businesses/me/photos/${encodeURIComponent(id)}`, {
    method: "DELETE",
    token,
  });
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

export async function getPortfolioUploadSignature(token: string): Promise<PortfolioUploadSignature> {
  return apiFetch<PortfolioUploadSignature>("/api/professionals/me/portfolio/upload-signature", {
    method: "POST",
    token,
  });
}

export async function uploadToCloudinary(
  signature: PortfolioUploadSignature,
  file: File,
): Promise<{ imageUrl: string; publicId: string }> {
  const formData = new FormData();
  formData.append("file", file);
  formData.append("api_key", signature.apiKey);
  formData.append("timestamp", String(signature.timestamp));
  formData.append("signature", signature.signature);
  formData.append("folder", signature.folder);
  formData.append("transformation", signature.transformation);

  const response = await fetch(
    `https://api.cloudinary.com/v1_1/${signature.cloudName}/image/upload`,
    { method: "POST", body: formData },
  );

  const data = await response.json();

  if (!response.ok) {
    throw new ApiError(response.status, data?.error?.message ?? "No se pudo subir la imagen.");
  }

  return { imageUrl: data.secure_url as string, publicId: data.public_id as string };
}

export async function addPortfolioPhoto(
  token: string,
  imageUrl: string,
  publicId: string,
  caption?: string,
): Promise<string> {
  const response = await apiFetch<RawAddPortfolioPhotoResponse>("/api/professionals/me/portfolio", {
    method: "POST",
    body: { imageUrl, publicId, caption },
    token,
  });
  return response.portfolioPhotoId.value;
}

export async function getMyPortfolio(token: string): Promise<PortfolioPhoto[]> {
  const response = await apiFetch<RawGetMyPortfolioResponse>("/api/professionals/me/portfolio", { token });
  return response.photos.map((photo) => ({ ...photo, id: photo.id.value }));
}

export async function deletePortfolioPhoto(token: string, id: string): Promise<void> {
  await apiFetch<void>(`/api/professionals/me/portfolio/${encodeURIComponent(id)}`, {
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

export async function adminLogin(username: string, password: string): Promise<string> {
  const response = await apiFetch<RawAdminLoginResponse>("/api/admin/login", {
    method: "POST",
    body: { username, password },
  });
  return response.token;
}

export async function getAdminPendingPhotos(token: string): Promise<AdminPendingPhoto[]> {
  const response = await apiFetch<RawGetAdminPendingPhotosResponse>("/api/admin/portfolio/pending", {
    token,
  });
  return response.photos.map((photo) => ({
    ...photo,
    id: photo.id.value,
    professionalId: photo.professionalId.value,
  }));
}

export async function approvePendingPhoto(token: string, id: string): Promise<void> {
  await apiFetch<void>(`/api/admin/portfolio/${encodeURIComponent(id)}/approve`, {
    method: "POST",
    token,
  });
}

export async function rejectPendingPhoto(token: string, id: string): Promise<void> {
  await apiFetch<void>(`/api/admin/portfolio/${encodeURIComponent(id)}/reject`, {
    method: "POST",
    token,
  });
}

export async function getAdminUploadSignature(
  token: string,
  photoId: string,
): Promise<PortfolioUploadSignature> {
  return apiFetch<PortfolioUploadSignature>(
    `/api/admin/portfolio/${encodeURIComponent(photoId)}/upload-signature`,
    { method: "POST", token },
  );
}

export async function replacePortfolioPhotoImage(
  token: string,
  photoId: string,
  imageUrl: string,
  publicId: string,
): Promise<void> {
  await apiFetch<void>(`/api/admin/portfolio/${encodeURIComponent(photoId)}/image`, {
    method: "PUT",
    body: { imageUrl, publicId },
    token,
  });
}

export async function getAdminPendingBusinessPhotos(token: string): Promise<AdminPendingBusinessPhoto[]> {
  const response = await apiFetch<RawGetAdminPendingBusinessPhotosResponse>(
    "/api/admin/business-photos/pending",
    { token },
  );
  return response.photos.map((photo) => ({
    ...photo,
    id: photo.id.value,
    businessId: photo.businessId.value,
  }));
}

export async function approvePendingBusinessPhoto(token: string, id: string): Promise<void> {
  await apiFetch<void>(`/api/admin/business-photos/${encodeURIComponent(id)}/approve`, {
    method: "POST",
    token,
  });
}

export async function rejectPendingBusinessPhoto(token: string, id: string): Promise<void> {
  await apiFetch<void>(`/api/admin/business-photos/${encodeURIComponent(id)}/reject`, {
    method: "POST",
    token,
  });
}

export async function getAdminBusinessPhotoUploadSignature(
  token: string,
  photoId: string,
): Promise<PortfolioUploadSignature> {
  return apiFetch<PortfolioUploadSignature>(
    `/api/admin/business-photos/${encodeURIComponent(photoId)}/upload-signature`,
    { method: "POST", token },
  );
}

export async function replaceBusinessPhotoImage(
  token: string,
  photoId: string,
  imageUrl: string,
  publicId: string,
): Promise<void> {
  await apiFetch<void>(`/api/admin/business-photos/${encodeURIComponent(photoId)}/image`, {
    method: "PUT",
    body: { imageUrl, publicId },
    token,
  });
}
