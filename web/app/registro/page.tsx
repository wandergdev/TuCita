"use client";

import { FormEvent, Suspense, useEffect, useState } from "react";
import { useSearchParams } from "next/navigation";
import Link from "next/link";
import { SiteHeader } from "@/components/layout/SiteHeader";
import { Container } from "@/components/ui/Container";
import { Card } from "@/components/ui/Card";
import { Field } from "@/components/ui/Field";
import { Input } from "@/components/ui/Input";
import { PasswordInput } from "@/components/ui/PasswordInput";
import { Select } from "@/components/ui/Select";
import { Button } from "@/components/ui/Button";
import { Alert } from "@/components/ui/Alert";
import { WorkingScheduleEditor } from "@/components/forms/WorkingScheduleEditor";
import { CATEGORY_LABELS } from "@/lib/labels";
import { toTimeOnlyString } from "@/lib/format";
import { ApiError, getBusinessInvite, registerProfessional } from "@/lib/api";
import { sanitizePhoneInput } from "@/lib/phone";
import { sanitizeSlugInput } from "@/lib/slug";
import { SITE_DOMAIN } from "@/lib/site";
import type { Category, DayOfWeek } from "@/types/api";

const DURATION_OPTIONS = [30, 45, 60, 90];

function RegistroContent() {
  const searchParams = useSearchParams();
  const inviteToken = searchParams.get("invite");

  const [businessName, setBusinessName] = useState<string | null>(null);
  const [inviteError, setInviteError] = useState<string | null>(null);

  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [slug, setSlug] = useState("");
  const [category, setCategory] = useState<Category>("Barber");
  const [appointmentDurationMinutes, setAppointmentDurationMinutes] = useState(30);
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [days, setDays] = useState<DayOfWeek[]>(["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"]);
  const [startTime, setStartTime] = useState("09:00");
  const [endTime, setEndTime] = useState("17:00");
  const [lunchBreakEnabled, setLunchBreakEnabled] = useState(false);
  const [lunchBreakStartTime, setLunchBreakStartTime] = useState("12:00");
  const [lunchBreakEndTime, setLunchBreakEndTime] = useState("13:00");

  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isRegistered, setIsRegistered] = useState(false);

  useEffect(() => {
    if (!inviteToken) return;

    let cancelled = false;

    getBusinessInvite(inviteToken)
      .then((info) => {
        if (!cancelled) setBusinessName(info.businessName);
      })
      .catch((err) => {
        if (!cancelled) {
          setInviteError(
            err instanceof ApiError ? err.message : "El enlace de invitación no es válido.",
          );
        }
      });

    return () => {
      cancelled = true;
    };
  }, [inviteToken]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    if (password !== confirmPassword) {
      setError("Las contraseñas no coinciden.");
      return;
    }

    if (days.length === 0) {
      setError("Selecciona al menos un día de trabajo.");
      return;
    }

    setIsSubmitting(true);
    try {
      await registerProfessional({
        fullName,
        email,
        phoneNumber,
        slug,
        category,
        appointmentDurationMinutes,
        workingSchedule: [
          {
            days,
            startTime: toTimeOnlyString(startTime),
            endTime: toTimeOnlyString(endTime),
            lunchBreakStartTime: lunchBreakEnabled ? toTimeOnlyString(lunchBreakStartTime) : undefined,
            lunchBreakEndTime: lunchBreakEnabled ? toTimeOnlyString(lunchBreakEndTime) : undefined,
          },
        ],
        password,
        inviteToken: inviteToken ?? undefined,
      });
      setIsRegistered(true);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo completar el registro.");
    } finally {
      setIsSubmitting(false);
    }
  }

  if (isRegistered) {
    return (
      <>
        <SiteHeader />
        <main className="flex flex-1 items-center justify-center py-12">
          <Container className="max-w-md">
            <h1 className="text-2xl font-semibold text-foreground">¡Ya casi!</h1>
            <Card className="mt-6">
              <Alert tone="success">
                Te enviamos un correo a <strong>{email}</strong> con un enlace para verificar tu
                cuenta. Debes verificarla antes de poder iniciar sesión.
              </Alert>
              <p className="mt-4 text-sm text-muted">
                ¿No te llegó? Revisa tu carpeta de spam, o solicita un nuevo enlace desde la{" "}
                <Link href="/login" className="font-medium text-accent hover:text-accent-hover">
                  pantalla de inicio de sesión
                </Link>
                .
              </p>
            </Card>
          </Container>
        </main>
      </>
    );
  }

  return (
    <>
      <SiteHeader />
      <main className="flex-1 py-12">
        <Container>
          <h1 className="text-2xl font-semibold text-foreground">
            {inviteToken ? "Únete como profesional" : "Registra tu negocio"}
          </h1>
          <p className="mt-1 text-sm text-muted">
            Completa tus datos para obtener tu página pública de reservas.
          </p>

          {inviteToken && businessName && (
            <Alert tone="success" className="mt-4">
              Te unes a <strong>{businessName}</strong>.
            </Alert>
          )}
          {inviteToken && inviteError && (
            <Alert tone="error" className="mt-4">
              {inviteError}
            </Alert>
          )}

          <Card className="mt-6">
            <form onSubmit={handleSubmit} className="flex flex-col gap-5">
              {error && <Alert tone="error">{error}</Alert>}

              <Field label="Nombre completo" htmlFor="fullName">
                <Input
                  id="fullName"
                  required
                  value={fullName}
                  onChange={(event) => setFullName(event.target.value)}
                />
              </Field>

              <div className="grid gap-4 sm:grid-cols-2">
                <Field label="Correo electrónico" htmlFor="email">
                  <Input
                    id="email"
                    type="email"
                    required
                    value={email}
                    onChange={(event) => setEmail(event.target.value)}
                  />
                </Field>
                <Field label="Teléfono" htmlFor="phoneNumber" hint="10 dígitos, ej. 8095551234">
                  <Input
                    id="phoneNumber"
                    type="tel"
                    inputMode="numeric"
                    maxLength={10}
                    required
                    value={phoneNumber}
                    onChange={(event) => setPhoneNumber(sanitizePhoneInput(event.target.value))}
                  />
                </Field>
              </div>

              <Field
                label="Slug (tu URL pública)"
                htmlFor="slug"
                hint={
                  slug
                    ? `${SITE_DOMAIN}/${slug}`
                    : "Solo minúsculas, números y guiones, sin tildes ni espacios. Ejemplo: jose-barber"
                }
              >
                <Input
                  id="slug"
                  required
                  value={slug}
                  onChange={(event) => setSlug(sanitizeSlugInput(event.target.value))}
                />
              </Field>

              <div className="grid gap-4 sm:grid-cols-2">
                <Field label="Categoría" htmlFor="category">
                  <Select
                    id="category"
                    value={category}
                    onChange={(event) => setCategory(event.target.value as Category)}
                  >
                    {Object.entries(CATEGORY_LABELS).map(([value, label]) => (
                      <option key={value} value={value}>
                        {label}
                      </option>
                    ))}
                  </Select>
                </Field>
                <Field label="Duración de la cita" htmlFor="duration">
                  <Select
                    id="duration"
                    value={appointmentDurationMinutes}
                    onChange={(event) => setAppointmentDurationMinutes(Number(event.target.value))}
                  >
                    {DURATION_OPTIONS.map((minutes) => (
                      <option key={minutes} value={minutes}>
                        {minutes} minutos
                      </option>
                    ))}
                  </Select>
                </Field>
              </div>

              <WorkingScheduleEditor
                selectedDays={days}
                onChangeDays={setDays}
                startTime={startTime}
                endTime={endTime}
                onChangeStartTime={setStartTime}
                onChangeEndTime={setEndTime}
                lunchBreakEnabled={lunchBreakEnabled}
                onToggleLunchBreak={setLunchBreakEnabled}
                lunchBreakStartTime={lunchBreakStartTime}
                lunchBreakEndTime={lunchBreakEndTime}
                onChangeLunchBreakStartTime={setLunchBreakStartTime}
                onChangeLunchBreakEndTime={setLunchBreakEndTime}
              />

              <div className="grid gap-4 sm:grid-cols-2">
                <Field label="Contraseña" htmlFor="password">
                  <PasswordInput
                    id="password"
                    required
                    minLength={8}
                    value={password}
                    onChange={(event) => setPassword(event.target.value)}
                  />
                </Field>
                <Field label="Confirmar contraseña" htmlFor="confirmPassword">
                  <PasswordInput
                    id="confirmPassword"
                    required
                    minLength={8}
                    value={confirmPassword}
                    onChange={(event) => setConfirmPassword(event.target.value)}
                  />
                </Field>
              </div>

              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? "Creando cuenta…" : "Crear mi cuenta"}
              </Button>

              <p className="text-center text-sm text-muted">
                ¿Ya tienes cuenta?{" "}
                <Link
                  href={inviteToken ? `/login?invite=${encodeURIComponent(inviteToken)}` : "/login"}
                  className="font-medium text-accent hover:text-accent-hover"
                >
                  {inviteToken ? "Inicia sesión para unirte" : "Inicia sesión"}
                </Link>
              </p>
            </form>
          </Card>
        </Container>
      </main>
    </>
  );
}

export default function RegistroPage() {
  return (
    <Suspense fallback={null}>
      <RegistroContent />
    </Suspense>
  );
}
