"use client";

import type { DayOfWeek } from "@/types/api";
import { DaySelector } from "@/components/forms/DaySelector";
import { LunchBreakFields } from "@/components/forms/LunchBreakFields";
import { Field } from "@/components/ui/Field";
import { Input } from "@/components/ui/Input";

interface WorkingScheduleEditorProps {
  selectedDays: DayOfWeek[];
  onChangeDays: (days: DayOfWeek[]) => void;
  startTime: string;
  endTime: string;
  onChangeStartTime: (value: string) => void;
  onChangeEndTime: (value: string) => void;
  lunchBreakEnabled: boolean;
  onToggleLunchBreak: (enabled: boolean) => void;
  lunchBreakStartTime: string;
  lunchBreakEndTime: string;
  onChangeLunchBreakStartTime: (value: string) => void;
  onChangeLunchBreakEndTime: (value: string) => void;
  error?: string;
}

export function WorkingScheduleEditor({
  selectedDays,
  onChangeDays,
  startTime,
  endTime,
  onChangeStartTime,
  onChangeEndTime,
  lunchBreakEnabled,
  onToggleLunchBreak,
  lunchBreakStartTime,
  lunchBreakEndTime,
  onChangeLunchBreakStartTime,
  onChangeLunchBreakEndTime,
  error,
}: WorkingScheduleEditorProps) {
  return (
    <div className="flex flex-col gap-3">
      <div>
        <span className="text-sm font-medium text-foreground">Días de trabajo</span>
        <div className="mt-2">
          <DaySelector selectedDays={selectedDays} onChangeDays={onChangeDays} />
        </div>
      </div>
      <div className="grid grid-cols-2 gap-3">
        <Field label="Hora de inicio" htmlFor="startTime">
          <Input
            id="startTime"
            type="time"
            value={startTime}
            onChange={(event) => onChangeStartTime(event.target.value)}
          />
        </Field>
        <Field label="Hora de fin" htmlFor="endTime">
          <Input
            id="endTime"
            type="time"
            value={endTime}
            onChange={(event) => onChangeEndTime(event.target.value)}
          />
        </Field>
      </div>
      <LunchBreakFields
        idPrefix="registro"
        enabled={lunchBreakEnabled}
        onToggle={onToggleLunchBreak}
        startTime={lunchBreakStartTime}
        endTime={lunchBreakEndTime}
        onChangeStartTime={onChangeLunchBreakStartTime}
        onChangeEndTime={onChangeLunchBreakEndTime}
      />
      {error && <p className="text-xs text-rose-600">{error}</p>}
    </div>
  );
}
