"use client";

import { Suspense, useEffect, useState } from "react";
import { useSearchParams } from "next/navigation";
import Link from "next/link";
import { SiteHeader } from "@/components/layout/SiteHeader";
import { Container } from "@/components/ui/Container";
import { Card } from "@/components/ui/Card";
import { Field } from "@/components/ui/Field";
import { Input } from "@/components/ui/Input";
import { Button } from "@/components/ui/Button";
import { Alert } from "@/components/ui/Alert";
import { ApiError, resendVerificationEmail, verifyEmail } from "@/lib/api";

type Status = "verifying" | "success" | "error" | "missing-token";

function VerificarCorreoContent() {
  const searchParams = useSearchParams();
  const token = searchParams.get("token");

  const [status, setStatus] = useState<Status>(token ? "verifying" : "missing-token");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const [resendEmail, setResendEmail] = useState("");
  const [isResending, setIsResending] = useState(false);
  const [resendSent, setResendSent] = useState(false);

  useEffect(() => {
    if (!token) return;

    let cancelled = false;

    verifyEmail(token)
      .then(() => {
        if (!cancelled) setStatus("success");
      })
      .catch((err) => {
        if (cancelled) return;
        setErrorMessage(
          err instanceof ApiError ? err.message : "No se pudo verificar tu correo.",
        );
        setStatus("error");
      });

    return () => {
      cancelled = true;
    };
  }, [token]);

  async function handleResend(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsResending(true);
    try {
      await resendVerificationEmail(resendEmail);
      setResendSent(true);
    } catch {
      setResendSent(true);
    } finally {
      setIsResending(false);
    }
  }

  return (
    <>
      <SiteHeader />
      <main className="flex flex-1 items-center justify-center py-12">
        <Container className="max-w-md">
          <h1 className="text-2xl font-semibold text-foreground">Verificación de correo</h1>

          <Card className="mt-6">
            {status === "verifying" && (
              <p className="text-sm text-muted">Verificando tu correo…</p>
            )}

            {status === "success" && (
              <div className="flex flex-col gap-4">
                <Alert tone="success">¡Tu correo fue verificado! Ya puedes iniciar sesión.</Alert>
                <Link href="/login">
                  <Button type="button" className="w-full">
                    Iniciar sesión
                  </Button>
                </Link>
              </div>
            )}

            {(status === "error" || status === "missing-token") && (
              <div className="flex flex-col gap-5">
                <Alert tone="error">
                  {status === "missing-token"
                    ? "El enlace de verificación no es válido."
                    : errorMessage}
                </Alert>

                {resendSent ? (
                  <p className="text-sm text-muted">
                    Si el correo existe y aún no ha sido verificado, te enviamos un nuevo enlace.
                    Revisa tu bandeja de entrada.
                  </p>
                ) : (
                  <form onSubmit={handleResend} className="flex flex-col gap-4">
                    <p className="text-sm text-muted">
                      Escribe tu correo para reenviar el enlace de verificación.
                    </p>
                    <Field label="Correo electrónico" htmlFor="resendEmail">
                      <Input
                        id="resendEmail"
                        type="email"
                        required
                        value={resendEmail}
                        onChange={(event) => setResendEmail(event.target.value)}
                      />
                    </Field>
                    <Button type="submit" disabled={isResending}>
                      {isResending ? "Enviando…" : "Reenviar correo de verificación"}
                    </Button>
                  </form>
                )}
              </div>
            )}
          </Card>
        </Container>
      </main>
    </>
  );
}

export default function VerificarCorreoPage() {
  return (
    <Suspense fallback={null}>
      <VerificarCorreoContent />
    </Suspense>
  );
}
