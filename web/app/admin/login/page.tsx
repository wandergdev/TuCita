"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import { Container } from "@/components/ui/Container";
import { Card } from "@/components/ui/Card";
import { Field } from "@/components/ui/Field";
import { Input } from "@/components/ui/Input";
import { PasswordInput } from "@/components/ui/PasswordInput";
import { Button } from "@/components/ui/Button";
import { Alert } from "@/components/ui/Alert";
import { ApiError, adminLogin } from "@/lib/api";
import { setAdminSession } from "@/lib/adminAuth";

export default function AdminLoginPage() {
  const router = useRouter();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const token = await adminLogin(username, password);
      setAdminSession({ token });
      router.push("/admin/portfolio");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo iniciar sesión.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="flex flex-1 items-center justify-center py-12">
      <Container className="max-w-md">
        <h1 className="text-2xl font-semibold text-foreground">Panel de administración</h1>
        <p className="mt-1 text-sm text-muted">Moderación de fotos de portafolio.</p>

        <Card className="mt-6">
          <form onSubmit={handleSubmit} className="flex flex-col gap-5">
            {error && <Alert tone="error">{error}</Alert>}

            <Field label="Usuario" htmlFor="username">
              <Input
                id="username"
                required
                autoComplete="username"
                value={username}
                onChange={(event) => setUsername(event.target.value)}
              />
            </Field>

            <Field label="Contraseña" htmlFor="password">
              <PasswordInput
                id="password"
                required
                autoComplete="current-password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
              />
            </Field>

            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Entrando…" : "Entrar"}
            </Button>
          </form>
        </Card>
      </Container>
    </main>
  );
}
