"use client";

import { FormEvent, useEffect, useState } from "react";
import { Card } from "@/components/ui/Card";
import { Field } from "@/components/ui/Field";
import { Input } from "@/components/ui/Input";
import { Button } from "@/components/ui/Button";
import { Alert } from "@/components/ui/Alert";
import { cn } from "@/lib/cn";
import { ApiError, bookAppointment, getAvailability } from "@/lib/api";
import { formatDateLabel, formatTimeLabel, todayAsDateOnly, toTimeOnlyString } from "@/lib/format";

interface BookingWidgetProps {
  slug: string;
}

export function BookingWidget({ slug }: BookingWidgetProps) {
  const [date, setDate] = useState(todayAsDateOnly());
  const [slots, setSlots] = useState<string[] | null>(null);
  const [isWorkingDay, setIsWorkingDay] = useState(true);
  const [slotsError, setSlotsError] = useState<string | null>(null);
  const [selectedSlot, setSelectedSlot] = useState<string | null>(null);

  const [customerName, setCustomerName] = useState("");
  const [customerPhone, setCustomerPhone] = useState("");
  const [bookingError, setBookingError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [confirmedSlot, setConfirmedSlot] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    setSlots(null);
    setSlotsError(null);
    setSelectedSlot(null);

    getAvailability(slug, date)
      .then((result) => {
        if (!cancelled) {
          setSlots(result.availableSlots);
          setIsWorkingDay(result.isWorkingDay);
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setSlotsError(
            err instanceof ApiError ? err.message : "No se pudo cargar la disponibilidad.",
          );
          setSlots([]);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [slug, date]);

  async function handleBook(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!selectedSlot) return;

    setBookingError(null);
    setIsSubmitting(true);
    try {
      await bookAppointment(slug, {
        customerName,
        customerPhone,
        date,
        startTime: toTimeOnlyString(selectedSlot),
      });
      setConfirmedSlot(selectedSlot);
    } catch (err) {
      setBookingError(err instanceof ApiError ? err.message : "No se pudo completar la reserva.");
    } finally {
      setIsSubmitting(false);
    }
  }

  if (confirmedSlot) {
    return (
      <Card>
        <Alert tone="success">
          Cita confirmada para el {formatDateLabel(date)} a las {formatTimeLabel(confirmedSlot)}
        </Alert>
      </Card>
    );
  }

  return (
    <Card className="flex flex-col gap-5">
      <Field label="Fecha" htmlFor="date">
        <Input
          id="date"
          type="date"
          min={todayAsDateOnly()}
          value={date}
          onChange={(event) => setDate(event.target.value)}
        />
      </Field>

      <div>
        <span className="text-sm font-medium text-foreground">Horarios disponibles</span>
        <div className="mt-2">
          {slots === null && <p className="text-sm text-muted">Cargando horarios…</p>}
          {slotsError && (
            <Alert tone="error" className="mb-2">
              {slotsError}
            </Alert>
          )}
          {slots !== null && slots.length === 0 && !slotsError && (
            isWorkingDay ? (
              <p className="text-sm text-muted">No hay horarios disponibles para esta fecha.</p>
            ) : (
              <div className="rounded-md border border-cat-blue/25 bg-cat-blue/8 px-4 py-3">
                <p className="text-base font-semibold text-cat-blue">Día no laborable.</p>
              </div>
            )
          )}
          {slots !== null && slots.length > 0 && (
            <div className="flex flex-wrap gap-2">
              {slots.map((slot) => (
                <button
                  key={slot}
                  type="button"
                  onClick={() => setSelectedSlot(slot)}
                  className={cn(
                    "rounded-md border px-3 py-1.5 text-sm font-medium transition-colors",
                    selectedSlot === slot
                      ? "border-accent bg-accent text-accent-foreground"
                      : "border-border text-foreground hover:bg-muted-background",
                  )}
                >
                  {formatTimeLabel(slot)}
                </button>
              ))}
            </div>
          )}
        </div>
      </div>

      {selectedSlot && (
        <form onSubmit={handleBook} className="flex flex-col gap-4 border-t border-border pt-5">
          {bookingError && <Alert tone="error">{bookingError}</Alert>}
          <Field label="Tu nombre" htmlFor="customerName">
            <Input
              id="customerName"
              required
              value={customerName}
              onChange={(event) => setCustomerName(event.target.value)}
            />
          </Field>
          <Field label="Tu teléfono" htmlFor="customerPhone">
            <Input
              id="customerPhone"
              required
              value={customerPhone}
              onChange={(event) => setCustomerPhone(event.target.value)}
            />
          </Field>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting
              ? "Reservando…"
              : `Reservar ${formatDateLabel(date)} a las ${formatTimeLabel(selectedSlot)}`}
          </Button>
        </form>
      )}
    </Card>
  );
}
