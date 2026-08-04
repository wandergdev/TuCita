"use client";

import { cn } from "@/lib/cn";
import { DAY_OPTIONS } from "@/lib/labels";
import type { DayOfWeek } from "@/types/api";

interface DaySelectorProps {
  selectedDays: DayOfWeek[];
  onChangeDays: (days: DayOfWeek[]) => void;
  disabledDays?: DayOfWeek[];
}

export function DaySelector({ selectedDays, onChangeDays, disabledDays = [] }: DaySelectorProps) {
  function toggleDay(day: DayOfWeek) {
    if (selectedDays.includes(day)) {
      onChangeDays(selectedDays.filter((d) => d !== day));
    } else {
      onChangeDays([...selectedDays, day]);
    }
  }

  return (
    <div className="flex flex-wrap gap-2">
      {DAY_OPTIONS.map((day) => {
        const active = selectedDays.includes(day.value);
        const disabled = !active && disabledDays.includes(day.value);
        return (
          <button
            key={day.value}
            type="button"
            disabled={disabled}
            onClick={() => toggleDay(day.value)}
            className={cn(
              "rounded-md border px-3 py-1.5 text-sm font-medium transition-colors",
              active
                ? "border-accent bg-accent text-accent-foreground"
                : disabled
                  ? "cursor-not-allowed border-border text-muted opacity-40"
                  : "border-border text-foreground hover:bg-muted-background",
            )}
          >
            {day.label}
          </button>
        );
      })}
    </div>
  );
}
