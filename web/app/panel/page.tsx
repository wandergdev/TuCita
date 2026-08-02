"use client";

import { useCallback, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Container } from "@/components/ui/Container";
import { Button } from "@/components/ui/Button";
import { Alert } from "@/components/ui/Alert";
import { Tabs } from "@/components/ui/Tabs";
import { ThemeToggle } from "@/components/ui/ThemeToggle";
import { AppointmentCard } from "@/components/dashboard/AppointmentCard";
import { ProfessionalHeader } from "@/components/dashboard/ProfessionalHeader";
import { ShareLinkCard } from "@/components/dashboard/ShareLinkCard";
import { StatsPanel } from "@/components/dashboard/StatsPanel";
import { STATUS_LABELS } from "@/lib/labels";
import { clearSession, getSession } from "@/lib/auth";
import {
  ApiError,
  cancelAppointment,
  completeAppointment,
  getMyAppointments,
  markAppointmentAsNoShow,
} from "@/lib/api";
import type { AppointmentStatus, AppointmentSummary, AuthSession } from "@/types/api";

const PAGE_SIZE = 50;

type TabValue = "Pendientes" | "Completadas" | "Canceladas" | "NoShows" | "Todas";

const TAB_STATUS: Record<TabValue, AppointmentStatus | undefined> = {
  Pendientes: "Scheduled",
  Completadas: "Completed",
  Canceladas: "Cancelled",
  NoShows: "NoShow",
  Todas: undefined,
};

const TAB_ITEMS: { value: TabValue; label: string }[] = [
  { value: "Pendientes", label: STATUS_LABELS.Scheduled },
  { value: "Completadas", label: STATUS_LABELS.Completed },
  { value: "Canceladas", label: STATUS_LABELS.Cancelled },
  { value: "NoShows", label: STATUS_LABELS.NoShow },
  { value: "Todas", label: "Todas" },
];

const EMPTY_STATE_TEXT: Record<TabValue, string> = {
  Pendientes: "No tienes citas pendientes.",
  Completadas: "Todavía no tienes citas completadas.",
  Canceladas: "No tienes citas canceladas.",
  NoShows: "No tienes citas marcadas como No Show.",
  Todas: "Todavía no tienes citas.",
};

export default function PanelPage() {
  const router = useRouter();
  const [session, setSessionState] = useState<AuthSession | undefined>(undefined);
  const [tab, setTab] = useState<TabValue>("Pendientes");
  const [appointments, setAppointments] = useState<AppointmentSummary[]>([]);
  const [hasMore, setHasMore] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [isLoadingMore, setIsLoadingMore] = useState(false);
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

  const loadFirstPage = useCallback(async (activeSession: AuthSession, activeTab: TabValue) => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await getMyAppointments(activeSession.token, {
        status: TAB_STATUS[activeTab],
        skip: 0,
        take: PAGE_SIZE,
      });
      setAppointments(result.appointments);
      setHasMore(result.hasMore);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudieron cargar tus citas.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    if (session) {
      loadFirstPage(session, tab);
    }
  }, [session, tab, loadFirstPage]);

  async function loadMore() {
    if (!session) return;
    setIsLoadingMore(true);
    setError(null);
    try {
      const result = await getMyAppointments(session.token, {
        status: TAB_STATUS[tab],
        skip: appointments.length,
        take: PAGE_SIZE,
      });
      setAppointments((current) => [...current, ...result.appointments]);
      setHasMore(result.hasMore);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudieron cargar más citas.");
    } finally {
      setIsLoadingMore(false);
    }
  }

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
      await loadFirstPage(session, tab);
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
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h1 className="text-2xl font-semibold text-foreground">Mis citas</h1>
            <p className="mt-1 text-sm text-muted">Gestiona tus citas programadas.</p>
          </div>
          <div className="flex items-center gap-2 self-end sm:self-auto">
            <ThemeToggle />
            <Button variant="outline" onClick={handleLogout}>
              Cerrar sesión
            </Button>
          </div>
        </div>

        <div className="mt-6">
          <ProfessionalHeader slug={session.slug} />
        </div>

        <div className="mt-4">
          <StatsPanel token={session.token} />
        </div>

        <div className="mt-6 rounded-lg border border-border bg-surface shadow-sm">
          <Tabs items={TAB_ITEMS} value={tab} onChange={setTab} />

          <div className="flex flex-col gap-3 p-4">
            {error && <Alert tone="error">{error}</Alert>}
            {isLoading && <p className="text-sm text-muted">Cargando citas…</p>}
            {!isLoading && appointments.length === 0 && (
              <p className="text-sm text-muted">{EMPTY_STATE_TEXT[tab]}</p>
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
            {!isLoading && hasMore && (
              <Button
                variant="outline"
                type="button"
                disabled={isLoadingMore}
                onClick={loadMore}
                className="self-center"
              >
                {isLoadingMore ? "Cargando…" : "Cargar más"}
              </Button>
            )}
          </div>
        </div>
      </Container>

      <ShareLinkCard slug={session.slug} />
    </main>
  );
}
