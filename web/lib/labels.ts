import type { AppointmentStatus, Category, DayOfWeek } from "@/types/api";

export const CATEGORY_LABELS: Record<Category, string> = {
  Barber: "Barbero",
  Stylist: "Estilista",
  Dentist: "Dentista",
  Psychologist: "Psicólogo/a",
};

export const STATUS_LABELS: Record<AppointmentStatus, string> = {
  Scheduled: "Programada",
  Completed: "Completada",
  Cancelled: "Cancelada",
  NoShow: "No Show",
};

export const STATUS_BADGE_CLASSES: Record<AppointmentStatus, string> = {
  Scheduled: "bg-blue-50 text-blue-700",
  Completed: "bg-emerald-50 text-emerald-700",
  Cancelled: "bg-slate-100 text-slate-500",
  NoShow: "bg-amber-50 text-amber-700",
};

export const DAY_OPTIONS: { value: DayOfWeek; label: string }[] = [
  { value: "Monday", label: "Lun" },
  { value: "Tuesday", label: "Mar" },
  { value: "Wednesday", label: "Mié" },
  { value: "Thursday", label: "Jue" },
  { value: "Friday", label: "Vie" },
  { value: "Saturday", label: "Sáb" },
  { value: "Sunday", label: "Dom" },
];
