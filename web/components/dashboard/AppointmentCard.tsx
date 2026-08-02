import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { STATUS_BADGE_CLASSES, STATUS_LABELS } from "@/lib/labels";
import { formatDateLabel, formatTimeLabel } from "@/lib/format";
import type { AppointmentSummary } from "@/types/api";

interface AppointmentCardProps {
  appointment: AppointmentSummary;
  onComplete: (id: string) => void;
  onCancel: (id: string) => void;
  onNoShow: (id: string) => void;
  isBusy: boolean;
}

export function AppointmentCard({
  appointment,
  onComplete,
  onCancel,
  onNoShow,
  isBusy,
}: AppointmentCardProps) {
  const isScheduled = appointment.status === "Scheduled";

  return (
    <div className="flex flex-col gap-3 rounded-lg border border-border p-4 sm:flex-row sm:items-center sm:justify-between">
      <div className="min-w-0">
        <div className="flex flex-wrap items-center gap-2">
          <p className="break-words font-medium text-foreground">{appointment.customerName}</p>
          <Badge className={STATUS_BADGE_CLASSES[appointment.status]}>
            {STATUS_LABELS[appointment.status]}
          </Badge>
        </div>
        <p className="text-sm text-muted">
          {formatDateLabel(appointment.date)} · {formatTimeLabel(appointment.startTime)} -{" "}
          {formatTimeLabel(appointment.endTime)}
        </p>
        <p className="text-sm text-muted">{appointment.customerPhone}</p>
      </div>
      {isScheduled && (
        <div className="flex flex-wrap gap-2">
          <Button variant="outline" disabled={isBusy} onClick={() => onComplete(appointment.id)}>
            Completar
          </Button>
          <Button variant="outline" disabled={isBusy} onClick={() => onNoShow(appointment.id)}>
            No Show
          </Button>
          <Button variant="danger" disabled={isBusy} onClick={() => onCancel(appointment.id)}>
            Cancelar
          </Button>
        </div>
      )}
    </div>
  );
}
