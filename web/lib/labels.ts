import type { AppointmentStatus, Category, DayOfWeek } from "@/types/api";

export const CATEGORY_LABELS: Record<Category, string> = {
  Barber: "Barbero",
  Stylist: "Estilista",
  Dentist: "Dentista",
  Psychologist: "Psicólogo/a",
};

/**
 * Texto del hero de la página pública de un negocio, según la categoría
 * del primer profesional de su equipo (ver [slug]/page.tsx).
 */
export const CATEGORY_TAGLINES: Record<Category, string> = {
  Barber: "Corte perfecto, sin filas ni llamadas: reserva tu cita en segundos.",
  Stylist: "Tu próximo look te espera. Reserva tu cita con nuestro equipo en segundos.",
  Dentist: "Cuida tu sonrisa sin complicaciones. Agenda tu cita dental cuando quieras.",
  Psychologist: "Date el espacio que mereces. Agenda tu sesión de forma fácil y privada.",
};

export const DEFAULT_HERO_TAGLINE = "Reserva tu cita en segundos, sin llamadas ni esperas.";

export const STATUS_LABELS: Record<AppointmentStatus, string> = {
  Scheduled: "Programada",
  Completed: "Completada",
  Cancelled: "Cancelada",
  NoShow: "No Show",
};

export const STATUS_BADGE_CLASSES: Record<AppointmentStatus, string> = {
  Scheduled: "bg-blue-50 text-blue-700 dark:bg-blue-500/15 dark:text-blue-300",
  Completed: "bg-emerald-50 text-emerald-700 dark:bg-emerald-500/15 dark:text-emerald-300",
  Cancelled: "bg-slate-100 text-slate-500 dark:bg-slate-500/15 dark:text-slate-300",
  NoShow: "bg-amber-50 text-amber-700 dark:bg-amber-500/15 dark:text-amber-300",
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
