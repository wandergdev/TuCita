import Link from "next/link";
import { SiteHeader } from "@/components/layout/SiteHeader";
import { Container } from "@/components/ui/Container";
import { Card } from "@/components/ui/Card";

const steps = [
  {
    title: "Regístrate en minutos",
    description:
      "Cuéntanos tu nombre, categoría, duración de tus citas y horario de trabajo.",
  },
  {
    title: "Comparte tu enlace",
    description: "Obtienes una URL propia, por ejemplo tucita.com/jose-barber.",
  },
  {
    title: "Recibe reservas solas",
    description:
      "Tus clientes eligen un horario disponible y agendan sin llamarte ni escribirte.",
  },
];

export default function Home() {
  return (
    <>
      <SiteHeader />
      <main className="flex-1">
        <Container className="max-w-3xl py-20 text-center">
          <h1 className="text-4xl font-semibold tracking-tight text-foreground sm:text-5xl">
            Agenda citas sin llamadas ni WhatsApp
          </h1>
          <p className="mx-auto mt-4 max-w-xl text-base text-muted">
            TuCita le da a tu negocio una página propia donde tus clientes ven tus
            horarios disponibles y reservan solos, en segundos.
          </p>
          <div className="mt-8 flex items-center justify-center gap-3">
            <Link
              href="/registro"
              className="rounded-md bg-accent px-5 py-2.5 text-sm font-medium text-accent-foreground hover:bg-accent-hover"
            >
              Registrar mi negocio
            </Link>
            <Link
              href="/login"
              className="rounded-md border border-border px-5 py-2.5 text-sm font-medium text-foreground hover:bg-muted-background"
            >
              Ya tengo cuenta
            </Link>
          </div>
        </Container>

        <Container className="max-w-4xl pb-20">
          <div className="grid gap-4 sm:grid-cols-3">
            {steps.map((step, index) => (
              <Card key={step.title}>
                <span className="text-sm font-medium text-accent">
                  {String(index + 1).padStart(2, "0")}
                </span>
                <h2 className="mt-2 text-base font-semibold text-foreground">
                  {step.title}
                </h2>
                <p className="mt-1 text-sm text-muted">{step.description}</p>
              </Card>
            ))}
          </div>
        </Container>
      </main>
    </>
  );
}
