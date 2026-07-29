"use client";

import { useCallback, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Container } from "@/components/ui/Container";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Alert } from "@/components/ui/Alert";
import { Input } from "@/components/ui/Input";
import { Field } from "@/components/ui/Field";
import { AppointmentCard } from "@/components/dashboard/AppointmentCard";
import { ShareLinkCard } from "@/components/dashboard/ShareLinkCard";
import { clearSession, getSession } from "@/lib/auth";
import {
  ApiError,
  cancelAppointment,
  completeAppointment,
  getMyAppointments,
  markAppointmentAsNoShow,
} from "@/lib/api";
import type { AppointmentSummary, AuthSession } from "@/types/api";

function sortAppointments(appointments: AppointmentSummary[]): AppointmentSummary[] {
  return appointments
    .slice()
    .sort((a, b) => `${a.date}${a.startTime}`.localeCompare(`${b.date}${b.startTime}`));
}

export default function PanelPage() {
  const router = useRouter();
  const [session, setSessionState] = useState<AuthSession | undefined>(undefined);
  const [appointments, setAppointments] = useState<AppointmentSummary[]>([]);
  const [dateFilter, setDateFilter] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  useEffect(() => {
    const current = getSession();
    if (!current) {
      router.replace("/login");
      return;
    }
    setSessionState(current);
  }, [router]);

  const loadAppointments = useCallback(async (activeSession: AuthSession, date: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await getMyAppointments(activeSession.token, date || undefined);
      setAppointments(sortAppointments(result));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudieron cargar tus citas.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    if (session) {
      loadAppointments(session, dateFilter);
    }
  }, [session, dateFilter, loadAppointments]);

  function handleLogout() {
    clearSession();
    router.push("/login");
  }

  async function runAction(id: string, action: (token: string, id: string) => Promise<void>) {
    if (!session) return;
    setBusyId(id);
    setError(null);
    try {
      await action(session.token, id);
      await loadAppointments(session, dateFilter);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo actualizar la cita.");
    } finally {
      setBusyId(null);
    }
  }

  if (session === undefined) {
    return null;
  }

  return (
    <main className="flex-1 py-10">
      <Container className="max-w-3xl">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-semibold text-foreground">Mis citas</h1>
            <p className="mt-1 text-sm text-muted">Gestiona tus citas programadas.</p>
          </div>
          <Button variant="outline" onClick={handleLogout}>
            Cerrar sesión
          </Button>
        </div>

        <div className="mt-6">
          <ShareLinkCard slug={session.slug} />
        </div>

        <Card className="mt-4">
          <Field
            label="Filtrar por fecha"
            htmlFor="dateFilter"
            hint="Deja vacío para ver todas tus citas"
          >
            <div className="flex gap-2">
              <Input
                id="dateFilter"
                type="date"
                value={dateFilter}
                onChange={(event) => setDateFilter(event.target.value)}
              />
              {dateFilter && (
                <Button variant="ghost" type="button" onClick={() => setDateFilter("")}>
                  Limpiar
                </Button>
              )}
            </div>
          </Field>
        </Card>

        <div className="mt-6 flex flex-col gap-3">
          {error && <Alert tone="error">{error}</Alert>}
          {isLoading && <p className="text-sm text-muted">Cargando citas…</p>}
          {!isLoading && appointments.length === 0 && (
            <Card>
              <p className="text-sm text-muted">
                Todavía no tienes citas{dateFilter ? " para esta fecha" : ""}. Comparte tu enlace
                de arriba con tus clientes para empezar a recibir reservas.
              </p>
            </Card>
          )}
          {appointments.map((appointment) => (
            <AppointmentCard
              key={appointment.id}
              appointment={appointment}
              isBusy={busyId === appointment.id}
              onComplete={(id) => runAction(id, completeAppointment)}
              onCancel={(id) => runAction(id, cancelAppointment)}
              onNoShow={(id) => runAction(id, markAppointmentAsNoShow)}
            />
          ))}
        </div>
      </Container>
    </main>
  );
}
