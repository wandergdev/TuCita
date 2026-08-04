"use client";

import { Field } from "@/components/ui/Field";
import { Input } from "@/components/ui/Input";

interface LunchBreakFieldsProps {
  enabled: boolean;
  onToggle: (enabled: boolean) => void;
  startTime: string;
  endTime: string;
  onChangeStartTime: (value: string) => void;
  onChangeEndTime: (value: string) => void;
  idPrefix: string;
}

export function LunchBreakFields({
  enabled,
  onToggle,
  startTime,
  endTime,
  onChangeStartTime,
  onChangeEndTime,
  idPrefix,
}: LunchBreakFieldsProps) {
  return (
    <div className="flex flex-col gap-3">
      <label className="flex items-center gap-2 text-sm text-foreground">
        <input
          type="checkbox"
          checked={enabled}
          onChange={(event) => onToggle(event.target.checked)}
          className="h-4 w-4 rounded border-border"
        />
        Tomo hora de almuerzo
      </label>
      {enabled && (
        <div className="grid grid-cols-2 gap-3">
          <Field label="Inicio de almuerzo" htmlFor={`${idPrefix}-lunch-start`}>
            <Input
              id={`${idPrefix}-lunch-start`}
              type="time"
              value={startTime}
              onChange={(event) => onChangeStartTime(event.target.value)}
            />
          </Field>
          <Field label="Fin de almuerzo" htmlFor={`${idPrefix}-lunch-end`}>
            <Input
              id={`${idPrefix}-lunch-end`}
              type="time"
              value={endTime}
              onChange={(event) => onChangeEndTime(event.target.value)}
            />
          </Field>
        </div>
      )}
    </div>
  );
}
