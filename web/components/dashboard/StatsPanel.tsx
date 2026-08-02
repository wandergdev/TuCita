"use client";

import { useEffect, useState } from "react";
import { Button } from "@/components/ui/Button";
import { Alert } from "@/components/ui/Alert";
import { cn } from "@/lib/cn";
import { ApiError, getAppointmentStats } from "@/lib/api";
import type { AppointmentStats, StatsRange } from "@/types/api";

const RANGE_OPTIONS: { value: StatsRange; label: string }[] = [
  { value: "Day", label: "Hoy" },
  { value: "Week", label: "Esta semana" },
  { value: "Month", label: "Este mes" },
];

interface StatsPanelProps {
  token: string;
}

export function StatsPanel({ token }: StatsPanelProps) {
  const [range, setRange] = useState<StatsRange>("Day");
  const [stats, setStats] = useState<AppointmentStats | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    setIsLoading(true);
    setError(null);

    getAppointmentStats(token, range)
      .then((result) => {
        if (!cancelled) setStats(result);
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err instanceof ApiError ? err.message : "No se pudieron cargar las métricas.");
        }
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [token, range]);

  return (
    <div className="rounded-lg border border-cat-aqua/25 bg-cat-aqua/8 p-6 shadow-sm">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <span className="h-2 w-2 rounded-full bg-cat-aqua" aria-hidden />
          <h2 className="text-lg font-semibold text-foreground">Resumen</h2>
        </div>
        <div className="flex gap-2">
          {RANGE_OPTIONS.map((option) => (
            <Button
              key={option.value}
              type="button"
              variant={range === option.value ? "primary" : "outline"}
              className={range === option.value ? undefined : "bg-surface"}
              onClick={() => setRange(option.value)}
            >
              {option.label}
            </Button>
          ))}
        </div>
      </div>

      {error && (
        <Alert tone="error" className="mt-4">
          {error}
        </Alert>
      )}

      {isLoading ? (
        <p className="mt-4 pl-4 text-sm text-muted">Cargando métricas…</p>
      ) : stats ? (
        <div className="mt-4 grid grid-cols-2 gap-4 sm:grid-cols-4">
          <StatTile
            label="Clientes atendidos"
            value={stats.completedCount}
            theme="border-accent/25 bg-accent/8"
            dotClassName="bg-accent"
          />
          <StatTile
            label="Clientes distintos"
            value={stats.distinctCustomersCount}
            theme="border-cat-aqua/25 bg-cat-aqua/8"
            dotClassName="bg-cat-aqua"
          />
          <StatTile
            label="No shows"
            value={stats.noShowCount}
            theme="border-cat-blue/25 bg-cat-blue/8"
            dotClassName="bg-cat-blue"
          />
          <StatTile
            label="Cancelados"
            value={stats.cancelledCount}
            theme="border-border bg-muted-background"
            dotClassName="bg-muted"
          />
        </div>
      ) : null}
    </div>
  );
}

function StatTile({
  label,
  value,
  theme,
  dotClassName,
}: {
  label: string;
  value: number;
  theme: string;
  dotClassName: string;
}) {
  return (
    <div className={cn("rounded-lg border p-4", theme)}>
      <p className="text-2xl font-semibold text-foreground sm:text-3xl">{value}</p>
      <div className="mt-1 flex items-center gap-1.5">
        <span className={cn("h-1.5 w-1.5 rounded-full", dotClassName)} aria-hidden />
        <p className="text-sm text-muted">{label}</p>
      </div>
    </div>
  );
}
