"use client";

import { FormEvent, Suspense, useState } from "react";
import { useSearchParams } from "next/navigation";
import Link from "next/link";
import { SiteHeader } from "@/components/layout/SiteHeader";
import { Container } from "@/components/ui/Container";
import { Card } from "@/components/ui/Card";
import { Field } from "@/components/ui/Field";
import { PasswordInput } from "@/components/ui/PasswordInput";
import { Button } from "@/components/ui/Button";
import { Alert } from "@/components/ui/Alert";
import { ApiError, resetBusinessPassword, resetPassword } from "@/lib/api";

function RestablecerPasswordContent() {
  const searchParams = useSearchParams();
  const token = searchParams.get("token");

  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [success, setSuccess] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    if (newPassword !== confirmPassword) {
      setError("Las contraseñas no coinciden.");
      return;
    }

    setIsSubmitting(true);

    // El token no dice si la cuenta es de profesional o de negocio.
    // Probamos primero como profesional; solo si el token no le
    // pertenece a ningún profesional (404) probamos como negocio. Si el
    // token sí le pertenece pero está vencido o hay otro error, ese es
    // el mensaje correcto y no hace falta seguir probando.
    try {
      await resetPassword(token!, newPassword);
      setSuccess(true);
      setIsSubmitting(false);
      return;
    } catch (professionalError) {
      if (!(professionalError instanceof ApiError) || professionalError.status !== 404) {
        setError(
          professionalError instanceof ApiError
            ? professionalError.message
            : "No se pudo restablecer tu contraseña.",
        );
        setIsSubmitting(false);
        return;
      }
    }

    try {
      await resetBusinessPassword(token!, newPassword);
      setSuccess(true);
    } catch (businessError) {
      setError(
        businessError instanceof ApiError
          ? businessError.message
          : "No se pudo restablecer tu contraseña.",
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <>
      <SiteHeader />
      <main className="flex flex-1 items-center justify-center py-12">
        <Container className="max-w-md">
          <h1 className="text-2xl font-semibold text-foreground">Restablecer contraseña</h1>

          <Card className="mt-6">
            {!token ? (
              <div className="flex flex-col gap-4">
                <Alert tone="error">El enlace para restablecer tu contraseña no es válido.</Alert>
                <Link href="/olvide-password">
                  <Button type="button" variant="outline" className="w-full">
                    Solicitar un enlace nuevo
                  </Button>
                </Link>
              </div>
            ) : success ? (
              <div className="flex flex-col gap-4">
                <Alert tone="success">Tu contraseña fue actualizada. Ya puedes iniciar sesión.</Alert>
                <Link href="/login">
                  <Button type="button" className="w-full">
                    Iniciar sesión
                  </Button>
                </Link>
              </div>
            ) : (
              <form onSubmit={handleSubmit} className="flex flex-col gap-5">
                {error && <Alert tone="error">{error}</Alert>}

                <Field label="Nueva contraseña" htmlFor="newPassword">
                  <PasswordInput
                    id="newPassword"
                    required
                    minLength={8}
                    value={newPassword}
                    onChange={(event) => setNewPassword(event.target.value)}
                  />
                </Field>

                <Field label="Confirmar contraseña" htmlFor="confirmPassword">
                  <PasswordInput
                    id="confirmPassword"
                    required
                    minLength={8}
                    value={confirmPassword}
                    onChange={(event) => setConfirmPassword(event.target.value)}
                  />
                </Field>

                <Button type="submit" disabled={isSubmitting}>
                  {isSubmitting ? "Guardando…" : "Restablecer contraseña"}
                </Button>
              </form>
            )}
          </Card>
        </Container>
      </main>
    </>
  );
}

export default function RestablecerPasswordPage() {
  return (
    <Suspense fallback={null}>
      <RestablecerPasswordContent />
    </Suspense>
  );
}
