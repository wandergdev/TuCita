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
import { ApiError, loginProfessional } from "@/lib/api";
import { setSession } from "@/lib/auth";

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const session = await loginProfessional({ email, password });
      setSession(session);
      router.push("/panel");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo iniciar sesión.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <>
      <SiteHeader />
      <main className="flex flex-1 items-center justify-center py-12">
        <Container className="max-w-md">
          <h1 className="text-2xl font-semibold text-foreground">Iniciar sesión</h1>
          <p className="mt-1 text-sm text-muted">
            Entra a tu panel para gestionar tus citas.
          </p>

          <Card className="mt-6">
            <form onSubmit={handleSubmit} className="flex flex-col gap-5">
              {error && <Alert tone="error">{error}</Alert>}

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

              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? "Entrando…" : "Entrar"}
              </Button>

              <p className="text-center text-sm text-muted">
                ¿No tienes cuenta?{" "}
                <Link href="/registro" className="font-medium text-accent hover:text-accent-hover">
                  Registra tu negocio
                </Link>
              </p>
            </form>
          </Card>
        </Container>
      </main>
    </>
  );
}
