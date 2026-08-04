"use client";

import { DaySelector } from "@/components/forms/DaySelector";
import { LunchBreakFields } from "@/components/forms/LunchBreakFields";
import { Button } from "@/components/ui/Button";
import { Field } from "@/components/ui/Field";
import { Input } from "@/components/ui/Input";
import type { DayOfWeek } from "@/types/api";

export interface ScheduleRuleDraft {
  days: DayOfWeek[];
  startTime: string;
  endTime: string;
  lunchBreakEnabled: boolean;
  lunchBreakStartTime: string;
  lunchBreakEndTime: string;
}

interface ScheduleRulesEditorProps {
  rules: ScheduleRuleDraft[];
  onChange: (rules: ScheduleRuleDraft[]) => void;
  error?: string;
}

export function ScheduleRulesEditor({ rules, onChange, error }: ScheduleRulesEditorProps) {
  function updateRule(index: number, patch: Partial<ScheduleRuleDraft>) {
    onChange(rules.map((rule, i) => (i === index ? { ...rule, ...patch } : rule)));
  }

  function addRule() {
    onChange([
      ...rules,
      {
        days: [],
        startTime: "09:00",
        endTime: "17:00",
        lunchBreakEnabled: false,
        lunchBreakStartTime: "12:00",
        lunchBreakEndTime: "13:00",
      },
    ]);
  }

  function removeRule(index: number) {
    onChange(rules.filter((_, i) => i !== index));
  }

  function daysUsedByOtherRules(index: number): DayOfWeek[] {
    return rules.flatMap((rule, i) => (i === index ? [] : rule.days));
  }

  return (
    <div className="flex flex-col gap-4">
      {rules.map((rule, index) => (
        <div key={index} className="flex flex-col gap-3 rounded-md border border-border p-3">
          <div className="flex items-center justify-between">
            <span className="text-sm font-medium text-foreground">
              {rules.length > 1 ? `Horario ${index + 1}` : "Días de trabajo"}
            </span>
            {rules.length > 1 && (
              <button
                type="button"
                onClick={() => removeRule(index)}
                className="text-xs font-medium text-rose-600 hover:underline"
              >
                Quitar
              </button>
            )}
          </div>
          <DaySelector
            selectedDays={rule.days}
            onChangeDays={(days) => updateRule(index, { days })}
            disabledDays={daysUsedByOtherRules(index)}
          />
          <div className="grid grid-cols-2 gap-3">
            <Field label="Hora de inicio" htmlFor={`schedule-rule-start-${index}`}>
              <Input
                id={`schedule-rule-start-${index}`}
                type="time"
                value={rule.startTime}
                onChange={(event) => updateRule(index, { startTime: event.target.value })}
              />
            </Field>
            <Field label="Hora de fin" htmlFor={`schedule-rule-end-${index}`}>
              <Input
                id={`schedule-rule-end-${index}`}
                type="time"
                value={rule.endTime}
                onChange={(event) => updateRule(index, { endTime: event.target.value })}
              />
            </Field>
          </div>
          <LunchBreakFields
            idPrefix={`schedule-rule-${index}`}
            enabled={rule.lunchBreakEnabled}
            onToggle={(enabled) => updateRule(index, { lunchBreakEnabled: enabled })}
            startTime={rule.lunchBreakStartTime}
            endTime={rule.lunchBreakEndTime}
            onChangeStartTime={(value) => updateRule(index, { lunchBreakStartTime: value })}
            onChangeEndTime={(value) => updateRule(index, { lunchBreakEndTime: value })}
          />
        </div>
      ))}
      {error && <p className="text-xs text-rose-600">{error}</p>}
      <Button type="button" variant="outline" onClick={addRule} className="self-start">
        + Agregar horario distinto
      </Button>
    </div>
  );
}
