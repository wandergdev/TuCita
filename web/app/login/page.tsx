"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { SiteHeader } from "@/components/layout/SiteHeader";
import { Container } from "@/components/ui/Container";
import { Card } from "@/components/ui/Card";
import { Field } from "@/components/ui/Field";
import { Input } from "@/components/ui/Input";
import { PasswordInput } from "@/components/ui/PasswordInput";
import { Button } from "@/components/ui/Button";
import { Alert } from "@/components/ui/Alert";
import {
  ApiError,
  loginBusiness,
  loginProfessional,
  resendBusinessVerificationEmail,
  resendVerificationEmail,
} from "@/lib/api";
import { setSession } from "@/lib/auth";

type PendingAccountKind = "professional" | "business";

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [pendingVerification, setPendingVerification] = useState<PendingAccountKind | null>(null);
  const [isResending, setIsResending] = useState(false);
  const [resendSent, setResendSent] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setPendingVerification(null);
    setResendSent(false);
    setIsSubmitting(true);

    // No le pedimos al usuario que elija si su cuenta es de profesional o
    // de negocio: probamos primero como profesional y, si esas
    // credenciales no existen, probamos como negocio. Cualquiera de los
    // dos que valide decide a dónde se redirige.
    try {
      const session = await loginProfessional({ email, password });
      setSession(session);
      router.push("/panel");
      return;
    } catch (professionalError) {
      if (professionalError instanceof ApiError && professionalError.status === 403) {
        setPendingVerification("professional");
        setError(professionalError.message);
        setIsSubmitting(false);
        return;
      }
    }

    try {
      const session = await loginBusiness({ email, password });
      setSession(session);
      router.push("/panel-negocio");
    } catch (businessError) {
      if (businessError instanceof ApiError && businessError.status === 403) {
        setPendingVerification("business");
      }
      setError(businessError instanceof ApiError ? businessError.message : "No se pudo iniciar sesión.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleResend() {
    setIsResending(true);
    try {
      if (pendingVerification === "business") {
        await resendBusinessVerificationEmail(email);
      } else {
        await resendVerificationEmail(email);
      }
    } finally {
      setIsResending(false);
      setResendSent(true);
    }
  }

  return (
    <>
      <SiteHeader />
      <main className="flex flex-1 items-center justify-center py-12">
        <Container className="max-w-md">
          <h1 className="text-2xl font-semibold text-foreground">Iniciar sesión</h1>
          <p className="mt-1 text-sm text-muted">
            Entra a tu panel, ya seas profesional o dueño de un negocio.
          </p>

          <Card className="mt-6">
            <form onSubmit={handleSubmit} className="flex flex-col gap-5">
              {error && (
                <div className="flex flex-col gap-3">
                  <Alert tone="error">{error}</Alert>
                  {pendingVerification &&
                    (resendSent ? (
                      <p className="text-sm text-muted">
                        Si el correo existe y aún no ha sido verificado, te enviamos un nuevo
                        enlace.
                      </p>
                    ) : (
                      <Button
                        type="button"
                        variant="outline"
                        disabled={isResending || !email}
                        onClick={handleResend}
                      >
                        {isResending ? "Enviando…" : "Reenviar correo de verificación"}
                      </Button>
                    ))}
                </div>
              )}

              <Field label="Correo electrónico" htmlFor="email">
                <Input
                  id="email"
                  type="email"
                  required
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                />
              </Field>

              <Field label="Contraseña" htmlFor="password">
                <PasswordInput
                  id="password"
                  required
                  value={password}
                  onChange={(event) => setPassword(event.target.value)}
                />
              </Field>

              <Link
                href="/olvide-password"
                className="-mt-3 self-end text-sm font-medium text-accent hover:text-accent-hover"
              >
                ¿Olvidaste tu contraseña?
              </Link>

              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? "Entrando…" : "Entrar"}
              </Button>

              <p className="text-center text-sm text-muted">
                ¿No tienes cuenta?{" "}
                <Link href="/registro" className="font-medium text-accent hover:text-accent-hover">
                  Regístrate como profesional
                </Link>{" "}
                o{" "}
                <Link
                  href="/registro-negocio"
                  className="font-medium text-accent hover:text-accent-hover"
                >
                  registra tu negocio
                </Link>
                .
              </p>
            </form>
          </Card>
        </Container>
      </main>
    </>
  );
}
