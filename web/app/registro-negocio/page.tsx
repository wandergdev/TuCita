"use client";

import { FormEvent, useState } from "react";
import Link from "next/link";
import { SiteHeader } from "@/components/layout/SiteHeader";
import { Container } from "@/components/ui/Container";
import { Card } from "@/components/ui/Card";
import { Field } from "@/components/ui/Field";
import { Input } from "@/components/ui/Input";
import { PasswordInput } from "@/components/ui/PasswordInput";
import { Button } from "@/components/ui/Button";
import { Alert } from "@/components/ui/Alert";
import { ApiError, registerBusiness } from "@/lib/api";
import { sanitizeSlugInput } from "@/lib/slug";
import { SITE_DOMAIN } from "@/lib/site";

export default function RegistroNegocioPage() {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [slug, setSlug] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isRegistered, setIsRegistered] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    if (password !== confirmPassword) {
      setError("Las contraseñas no coinciden.");
      return;
    }

    setIsSubmitting(true);
    try {
      await registerBusiness({ name, email, slug, password });
      setIsRegistered(true);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo completar el registro.");
    } finally {
      setIsSubmitting(false);
    }
  }

  if (isRegistered) {
    return (
      <>
        <SiteHeader />
        <main className="flex flex-1 items-center justify-center py-12">
          <Container className="max-w-md">
            <h1 className="text-2xl font-semibold text-foreground">¡Ya casi!</h1>
            <Card className="mt-6">
              <Alert tone="success">
                Te enviamos un correo a <strong>{email}</strong> con un enlace para verificar tu
                cuenta. Debes verificarla antes de poder iniciar sesión.
              </Alert>
              <p className="mt-4 text-sm text-muted">
                Una vez verificada, entra a{" "}
                <Link href="/login" className="font-medium text-accent hover:text-accent-hover">
                  tu panel de negocio
                </Link>{" "}
                para invitar a tus profesionales.
              </p>
            </Card>
          </Container>
        </main>
      </>
    );
  }

  return (
    <>
      <SiteHeader />
      <main className="flex flex-1 items-center justify-center py-12">
        <Container className="max-w-md">
          <h1 className="text-2xl font-semibold text-foreground">Registra tu negocio</h1>
          <p className="mt-1 text-sm text-muted">
            Después podrás invitar a cada profesional para que se una con su propia cuenta.
          </p>

          <Card className="mt-6">
            <form onSubmit={handleSubmit} className="flex flex-col gap-5">
              {error && <Alert tone="error">{error}</Alert>}

              <Field label="Nombre del negocio" htmlFor="name">
                <Input
                  id="name"
                  required
                  value={name}
                  onChange={(event) => setName(event.target.value)}
                />
              </Field>

              <Field label="Correo electrónico" htmlFor="email">
                <Input
                  id="email"
                  type="email"
                  required
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                />
              </Field>

              <Field
                label="Slug (tu URL pública)"
                htmlFor="slug"
                hint={
                  slug
                    ? `${SITE_DOMAIN}/${slug}`
                    : "Solo minúsculas, números y guiones, sin tildes ni espacios. Ejemplo: peluqueria-lisbeth"
                }
              >
                <Input
                  id="slug"
                  required
                  value={slug}
                  onChange={(event) => setSlug(sanitizeSlugInput(event.target.value))}
                />
              </Field>

              <div className="grid gap-4 sm:grid-cols-2">
                <Field label="Contraseña" htmlFor="password">
                  <PasswordInput
                    id="password"
                    required
                    minLength={8}
                    value={password}
                    onChange={(event) => setPassword(event.target.value)}
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
              </div>

              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? "Creando cuenta…" : "Crear mi negocio"}
              </Button>

              <p className="text-center text-sm text-muted">
                ¿Ya tienes cuenta?{" "}
                <Link href="/login" className="font-medium text-accent hover:text-accent-hover">
                  Inicia sesión
                </Link>
              </p>
            </form>
          </Card>
        </Container>
      </main>
    </>
  );
}
