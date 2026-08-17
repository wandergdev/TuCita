export function toTimeOnlyString(value: string): string {
  return value.length === 5 ? `${value}:00` : value;
}

export function formatTimeLabel(value: string): string {
  const [hours, minutes] = value.split(":").map(Number);
  const date = new Date();
  date.setHours(hours, minutes, 0, 0);
  return new Intl.DateTimeFormat("es-DO", { hour: "numeric", minute: "2-digit" }).format(date);
}

export function formatDateLabel(value: string): string {
  const date = new Date(`${value}T00:00:00`);
  return new Intl.DateTimeFormat("es-DO", {
    weekday: "long",
    day: "numeric",
    month: "long",
  }).format(date);
}

export function hasAppointmentEnded(date: string, endTime: string): boolean {
  return new Date(`${date}T${endTime}`) <= new Date();
}

export function todayAsDateOnly(): string {
  const now = new Date();
  const offset = now.getTimezoneOffset();
  const local = new Date(now.getTime() - offset * 60 * 1000);
  return local.toISOString().slice(0, 10);
}
