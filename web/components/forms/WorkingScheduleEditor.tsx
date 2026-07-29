"use client";

import { cn } from "@/lib/cn";
import { DAY_OPTIONS } from "@/lib/labels";
import type { DayOfWeek } from "@/types/api";
import { Field } from "@/components/ui/Field";
import { Input } from "@/components/ui/Input";

interface WorkingScheduleEditorProps {
  selectedDays: DayOfWeek[];
  onChangeDays: (days: DayOfWeek[]) => void;
  startTime: string;
  endTime: string;
  onChangeStartTime: (value: string) => void;
  onChangeEndTime: (value: string) => void;
  error?: string;
}

export function WorkingScheduleEditor({
  selectedDays,
  onChangeDays,
  startTime,
  endTime,
  onChangeStartTime,
  onChangeEndTime,
  error,
}: WorkingScheduleEditorProps) {
  function toggleDay(day: DayOfWeek) {
    if (selectedDays.includes(day)) {
      onChangeDays(selectedDays.filter((d) => d !== day));
    } else {
      onChangeDays([...selectedDays, day]);
    }
  }

  return (
    <div className="flex flex-col gap-3">
      <div>
        <span className="text-sm font-medium text-foreground">Días de trabajo</span>
        <div className="mt-2 flex flex-wrap gap-2">
          {DAY_OPTIONS.map((day) => {
            const active = selectedDays.includes(day.value);
            return (
              <button
                key={day.value}
                type="button"
                onClick={() => toggleDay(day.value)}
                className={cn(
                  "rounded-md border px-3 py-1.5 text-sm font-medium transition-colors",
                  active
                    ? "border-accent bg-accent text-accent-foreground"
                    : "border-border text-foreground hover:bg-muted-background",
                )}
              >
                {day.label}
              </button>
            );
          })}
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
      {error && <p className="text-xs text-rose-600">{error}</p>}
    </div>
  );
}
