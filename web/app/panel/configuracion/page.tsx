"use client";

import { useCallback, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { Container } from "@/components/ui/Container";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Alert } from "@/components/ui/Alert";
import { Field } from "@/components/ui/Field";
import { Input } from "@/components/ui/Input";
import { PasswordInput } from "@/components/ui/PasswordInput";
import { Tabs } from "@/components/ui/Tabs";
import { LunchBreakFields } from "@/components/forms/LunchBreakFields";
import { ScheduleRulesEditor, type ScheduleRuleDraft } from "@/components/forms/ScheduleRulesEditor";
import { getSession } from "@/lib/auth";
import { formatDateLabel, formatTimeLabel, todayAsDateOnly, toTimeOnlyString } from "@/lib/format";
import {
  ApiError,
  changePassword,
  createScheduleException,
  deleteScheduleException,
  getMyWorkingSchedule,
  getScheduleExceptions,
  updateWorkingSchedule,
} from "@/lib/api";
import type { AuthSession, ScheduleException, WorkingScheduleRuleRequest } from "@/types/api";

type TabValue = "Horarios" | "Contraseña";

const TAB_ITEMS: { value: TabValue; label: string }[] = [
  { value: "Horarios", label: "Horarios" },
  { value: "Contraseña", label: "Contraseña" },
];

export default function ConfiguracionPage() {
  const router = useRouter();
  const [session, setSessionState] = useState<AuthSession | undefined>(undefined);
  const [tab, setTab] = useState<TabValue>("Horarios");
  const [exceptionsVersion, setExceptionsVersion] = useState(0);

  useEffect(() => {
    const current = getSession();
    if (!current) {
      router.replace("/login");
      return;
    }
    setSessionState(current);
  }, [router]);

  if (session === undefined) {
    return null;
  }

  return (
    <main className="flex-1 py-10">
      <Container className="max-w-3xl">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-semibold text-foreground">Configuración</h1>
            <p className="mt-1 text-sm text-muted">Administra tu horario y tu contraseña.</p>
          </div>
          <Link href="/panel">
            <Button variant="outline">Volver</Button>
          </Link>
        </div>

        <div className="mt-6 rounded-lg border border-border bg-surface shadow-sm">
          <Tabs items={TAB_ITEMS} value={tab} onChange={setTab} />

          <div className="flex flex-col gap-6 p-4">
            {tab === "Horarios" && (
              <>
                <WorkingScheduleSection token={session.token} />
                <SpecialDateSection
                  token={session.token}
                  onCreated={() => setExceptionsVersion((v) => v + 1)}
                />
                <VacationRangeSection
                  token={session.token}
                  onCreated={() => setExceptionsVersion((v) => v + 1)}
                />
                <ScheduleExceptionsSection token={session.token} version={exceptionsVersion} />
              </>
            )}

            {tab === "Contraseña" && <ChangePasswordSection token={session.token} />}
          </div>
        </div>
      </Container>
    </main>
  );
}

function ChangePasswordSection({ token }: { token: string }) {
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  async function handleSave() {
    setError(null);
    setSuccess(false);

    if (newPassword !== confirmPassword) {
      setError("Las contraseñas no coinciden.");
      return;
    }

    if (newPassword.length < 8) {
      setError("La contraseña debe tener al menos 8 caracteres.");
      return;
    }

    setIsSaving(true);
    try {
      await changePassword(token, currentPassword, newPassword);
      setSuccess(true);
      setCurrentPassword("");
      setNewPassword("");
      setConfirmPassword("");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo cambiar la contraseña.");
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Card className="flex flex-col gap-4">
      <div>
        <h2 className="text-lg font-semibold text-foreground">Cambiar contraseña</h2>
        <p className="mt-1 text-sm text-muted">
          Escribe tu contraseña actual y la nueva contraseña que quieres usar.
        </p>
      </div>

      {error && <Alert tone="error">{error}</Alert>}
      {success && <Alert tone="success">Contraseña actualizada.</Alert>}

      <Field label="Contraseña actual" htmlFor="current-password">
        <PasswordInput
          id="current-password"
          value={currentPassword}
          onChange={(event) => setCurrentPassword(event.target.value)}
        />
      </Field>
      <Field label="Nueva contraseña" htmlFor="new-password">
        <PasswordInput
          id="new-password"
          minLength={8}
          value={newPassword}
          onChange={(event) => setNewPassword(event.target.value)}
        />
      </Field>
      <Field label="Confirmar nueva contraseña" htmlFor="confirm-password">
        <PasswordInput
          id="confirm-password"
          minLength={8}
          value={confirmPassword}
          onChange={(event) => setConfirmPassword(event.target.value)}
        />
      </Field>
      <Button
        type="button"
        disabled={isSaving || !currentPassword || !newPassword || !confirmPassword}
        onClick={handleSave}
        className="self-start"
      >
        {isSaving ? "Guardando…" : "Cambiar contraseña"}
      </Button>
    </Card>
  );
}

function ruleToDraft(rule: WorkingScheduleRuleRequest): ScheduleRuleDraft {
  return {
    days: rule.days,
    startTime: rule.startTime.slice(0, 5),
    endTime: rule.endTime.slice(0, 5),
    lunchBreakEnabled: Boolean(rule.lunchBreakStartTime && rule.lunchBreakEndTime),
    lunchBreakStartTime: rule.lunchBreakStartTime?.slice(0, 5) ?? "12:00",
    lunchBreakEndTime: rule.lunchBreakEndTime?.slice(0, 5) ?? "13:00",
  };
}

function WorkingScheduleSection({ token }: { token: string }) {
  const [rules, setRules] = useState<ScheduleRuleDraft[] | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    getMyWorkingSchedule(token)
      .then((result) => setRules(result.map(ruleToDraft)))
      .catch((err) => setError(err instanceof ApiError ? err.message : "No se pudo cargar tu horario."))
      .finally(() => setIsLoading(false));
  }, [token]);

  async function handleSave() {
    if (!rules) return;
    setError(null);
    setSuccess(false);

    if (rules.length === 0 || rules.some((rule) => rule.days.length === 0)) {
      setError("Cada horario debe tener al menos un día seleccionado.");
      return;
    }

    setIsSaving(true);
    try {
      await updateWorkingSchedule(
        token,
        rules.map((rule) => ({
          days: rule.days,
          startTime: toTimeOnlyString(rule.startTime),
          endTime: toTimeOnlyString(rule.endTime),
          lunchBreakStartTime: rule.lunchBreakEnabled
            ? toTimeOnlyString(rule.lunchBreakStartTime)
            : undefined,
          lunchBreakEndTime: rule.lunchBreakEnabled
            ? toTimeOnlyString(rule.lunchBreakEndTime)
            : undefined,
        })),
      );
      setSuccess(true);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo guardar el horario.");
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Card className="flex flex-col gap-4">
      <div>
        <h2 className="text-lg font-semibold text-foreground">Horario habitual</h2>
        <p className="mt-1 text-sm text-muted">
          Agrega o quita días y define el rango de hora de cada uno.
        </p>
      </div>

      {isLoading && <p className="text-sm text-muted">Cargando horario…</p>}
      {error && <Alert tone="error">{error}</Alert>}
      {success && <Alert tone="success">Horario actualizado.</Alert>}

      {rules && (
        <>
          <ScheduleRulesEditor rules={rules} onChange={setRules} />
          <Button type="button" disabled={isSaving} onClick={handleSave} className="self-start">
            {isSaving ? "Guardando…" : "Guardar horario"}
          </Button>
        </>
      )}
    </Card>
  );
}

function SpecialDateSection({ token, onCreated }: { token: string; onCreated: () => void }) {
  const [date, setDate] = useState(todayAsDateOnly());
  const [startTime, setStartTime] = useState("09:00");
  const [endTime, setEndTime] = useState("17:00");
  const [lunchBreakEnabled, setLunchBreakEnabled] = useState(false);
  const [lunchBreakStartTime, setLunchBreakStartTime] = useState("12:00");
  const [lunchBreakEndTime, setLunchBreakEndTime] = useState("13:00");
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  async function handleSave() {
    setError(null);
    setSuccess(false);
    setIsSaving(true);
    try {
      await createScheduleException(token, {
        startDate: date,
        endDate: date,
        startTime: toTimeOnlyString(startTime),
        endTime: toTimeOnlyString(endTime),
        lunchBreakStartTime: lunchBreakEnabled ? toTimeOnlyString(lunchBreakStartTime) : undefined,
        lunchBreakEndTime: lunchBreakEnabled ? toTimeOnlyString(lunchBreakEndTime) : undefined,
      });
      setSuccess(true);
      onCreated();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo guardar el horario especial.");
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Card className="flex flex-col gap-4">
      <div>
        <h2 className="text-lg font-semibold text-foreground">Horario especial para un día</h2>
        <p className="mt-1 text-sm text-muted">
          Ese día trabajarás, pero con un horario distinto al habitual.
        </p>
      </div>

      {error && <Alert tone="error">{error}</Alert>}
      {success && <Alert tone="success">Horario especial guardado.</Alert>}

      <Field label="Fecha" htmlFor="special-date">
        <Input
          id="special-date"
          type="date"
          min={todayAsDateOnly()}
          value={date}
          onChange={(event) => setDate(event.target.value)}
        />
      </Field>
      <div className="grid grid-cols-2 gap-3">
        <Field label="Hora de inicio" htmlFor="special-start">
          <Input
            id="special-start"
            type="time"
            value={startTime}
            onChange={(event) => setStartTime(event.target.value)}
          />
        </Field>
        <Field label="Hora de fin" htmlFor="special-end">
          <Input
            id="special-end"
            type="time"
            value={endTime}
            onChange={(event) => setEndTime(event.target.value)}
          />
        </Field>
      </div>
      <LunchBreakFields
        idPrefix="special-date"
        enabled={lunchBreakEnabled}
        onToggle={setLunchBreakEnabled}
        startTime={lunchBreakStartTime}
        endTime={lunchBreakEndTime}
        onChangeStartTime={setLunchBreakStartTime}
        onChangeEndTime={setLunchBreakEndTime}
      />
      <Button type="button" disabled={isSaving} onClick={handleSave} className="self-start">
        {isSaving ? "Guardando…" : "Guardar horario especial"}
      </Button>
    </Card>
  );
}

function VacationRangeSection({ token, onCreated }: { token: string; onCreated: () => void }) {
  const [startDate, setStartDate] = useState(todayAsDateOnly());
  const [endDate, setEndDate] = useState(todayAsDateOnly());
  const [message, setMessage] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  async function handleSave() {
    setError(null);
    setSuccess(false);

    if (endDate < startDate) {
      setError("La fecha de fin debe ser igual o posterior a la fecha de inicio.");
      return;
    }

    setIsSaving(true);
    try {
      await createScheduleException(token, {
        startDate,
        endDate,
        message: message.trim() || undefined,
      });
      setSuccess(true);
      setMessage("");
      onCreated();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudieron guardar los días no laborables.");
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Card className="flex flex-col gap-4">
      <div>
        <h2 className="text-lg font-semibold text-foreground">Días no laborables</h2>
        <p className="mt-1 text-sm text-muted">
          Marca un rango de días en el que no estarás disponible (vacaciones u otro motivo).
        </p>
      </div>

      {error && <Alert tone="error">{error}</Alert>}
      {success && <Alert tone="success">Días no laborables guardados.</Alert>}

      <div className="grid grid-cols-2 gap-3">
        <Field label="Desde" htmlFor="vacation-start">
          <Input
            id="vacation-start"
            type="date"
            min={todayAsDateOnly()}
            value={startDate}
            onChange={(event) => setStartDate(event.target.value)}
          />
        </Field>
        <Field label="Hasta" htmlFor="vacation-end">
          <Input
            id="vacation-end"
            type="date"
            min={startDate}
            value={endDate}
            onChange={(event) => setEndDate(event.target.value)}
          />
        </Field>
      </div>
      <Field label="Mensaje (opcional)" htmlFor="vacation-message">
        <textarea
          id="vacation-message"
          rows={2}
          maxLength={300}
          value={message}
          onChange={(event) => setMessage(event.target.value)}
          placeholder="No estaré laborando por motivos de vacaciones."
          className="w-full rounded-md border border-border bg-surface px-3 py-2 text-base text-foreground placeholder:text-muted focus:border-accent focus:outline-none focus:ring-1 focus:ring-accent sm:text-sm"
        />
      </Field>
      <Button type="button" disabled={isSaving} onClick={handleSave} className="self-start">
        {isSaving ? "Guardando…" : "Guardar días no laborables"}
      </Button>
    </Card>
  );
}

function ScheduleExceptionsSection({ token, version }: { token: string; version: number }) {
  const [exceptions, setExceptions] = useState<ScheduleException[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  const load = useCallback(() => {
    getScheduleExceptions(token)
      .then(setExceptions)
      .catch((err) =>
        setError(err instanceof ApiError ? err.message : "No se pudieron cargar las excepciones."),
      );
  }, [token]);

  useEffect(() => {
    load();
  }, [load, version]);

  async function handleDelete(id: string) {
    setBusyId(id);
    setError(null);
    try {
      await deleteScheduleException(token, id);
      load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo eliminar la excepción.");
    } finally {
      setBusyId(null);
    }
  }

  return (
    <Card className="flex flex-col gap-4">
      <h2 className="text-lg font-semibold text-foreground">Próximas excepciones</h2>

      {error && <Alert tone="error">{error}</Alert>}
      {exceptions === null && <p className="text-sm text-muted">Cargando…</p>}
      {exceptions !== null && exceptions.length === 0 && (
        <p className="text-sm text-muted">No tienes excepciones de horario próximas.</p>
      )}

      {exceptions && exceptions.length > 0 && (
        <div className="flex flex-col gap-3">
          {exceptions.map((exception) => (
            <div
              key={exception.id}
              className="flex items-start justify-between gap-3 rounded-md border border-border p-3"
            >
              <div>
                <p className="text-sm font-medium text-foreground">
                  {exception.startDate === exception.endDate
                    ? formatDateLabel(exception.startDate)
                    : `${formatDateLabel(exception.startDate)} — ${formatDateLabel(exception.endDate)}`}
                </p>
                <p className="text-sm text-muted">
                  {exception.startTime && exception.endTime
                    ? `Horario especial: ${formatTimeLabel(exception.startTime)} - ${formatTimeLabel(exception.endTime)}`
                    : "No laborable"}
                </p>
                {exception.lunchBreakStartTime && exception.lunchBreakEndTime && (
                  <p className="text-sm text-muted">
                    Almuerzo: {formatTimeLabel(exception.lunchBreakStartTime)} -{" "}
                    {formatTimeLabel(exception.lunchBreakEndTime)}
                  </p>
                )}
                {exception.message && <p className="mt-1 text-sm text-muted">{exception.message}</p>}
              </div>
              <Button
                type="button"
                variant="outline"
                disabled={busyId === exception.id}
                onClick={() => handleDelete(exception.id)}
              >
                Eliminar
              </Button>
            </div>
          ))}
        </div>
      )}
    </Card>
  );
}
