"use client";

import { FormEvent, useState } from "react";
import Link from "next/link";
import { SiteHeader } from "@/components/layout/SiteHeader";
import { Container } from "@/components/ui/Container";
import { Card } from "@/components/ui/Card";
import { Field } from "@/components/ui/Field";
import { Input } from "@/components/ui/Input";
import { Button } from "@/components/ui/Button";
import { Alert } from "@/components/ui/Alert";
import { forgotBusinessPassword, forgotPassword } from "@/lib/api";

export default function OlvidePasswordPage() {
  const [email, setEmail] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [sent, setSent] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);
    // No sabemos si esta cuenta es de profesional o de negocio (y no lo
    // preguntamos): probamos las dos vías, y cualquiera de las dos que
    // encuentre el correo manda el enlace. La respuesta al usuario es la
    // misma sin importar cuál coincidió, para no revelar qué tipo de
    // cuenta existe con ese correo.
    await Promise.allSettled([forgotPassword(email), forgotBusinessPassword(email)]);
    setIsSubmitting(false);
    setSent(true);
  }

  return (
    <>
      <SiteHeader />
      <main className="flex flex-1 items-center justify-center py-12">
        <Container className="max-w-md">
          <h1 className="text-2xl font-semibold text-foreground">Olvidé mi contraseña</h1>
          <p className="mt-1 text-sm text-muted">
            Escribe tu correo y te enviamos un enlace para definir una nueva contraseña.
          </p>

          <Card className="mt-6">
            {sent ? (
              <div className="flex flex-col gap-4">
                <Alert tone="success">
                  Si el correo existe, te enviamos un enlace para restablecer tu contraseña.
                  Revisa tu bandeja de entrada.
                </Alert>
                <Link href="/login">
                  <Button type="button" variant="outline" className="w-full">
                    Volver a iniciar sesión
                  </Button>
                </Link>
              </div>
            ) : (
              <form onSubmit={handleSubmit} className="flex flex-col gap-5">
                <Field label="Correo electrónico" htmlFor="email">
                  <Input
                    id="email"
                    type="email"
                    required
                    value={email}
                    onChange={(event) => setEmail(event.target.value)}
                  />
                </Field>

                <Button type="submit" disabled={isSubmitting}>
                  {isSubmitting ? "Enviando…" : "Enviar enlace"}
                </Button>

                <p className="text-center text-sm text-muted">
                  <Link href="/login" className="font-medium text-accent hover:text-accent-hover">
                    Volver a iniciar sesión
                  </Link>
                </p>
              </form>
            )}
          </Card>
        </Container>
      </main>
    </>
  );
}
