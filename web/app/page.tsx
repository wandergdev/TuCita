import Link from "next/link";
import { SiteHeader } from "@/components/layout/SiteHeader";
import { Container } from "@/components/ui/Container";

const steps = [
  {
    title: "Regístrate en minutos",
    description:
      "Cuéntanos tu nombre, categoría, duración de tus citas y horario de trabajo.",
    theme: "border-accent/25 bg-accent/8",
    dotClassName: "bg-accent",
  },
  {
    title: "Comparte tu enlace",
    description: "Obtienes una URL propia, por ejemplo tucita.com/jose-barber.",
    theme: "border-cat-aqua/25 bg-cat-aqua/8",
    dotClassName: "bg-cat-aqua",
  },
  {
    title: "Recibe reservas solas",
    description:
      "Tus clientes eligen un horario disponible y agendan sin llamarte ni escribirte.",
    theme: "border-cat-blue/25 bg-cat-blue/8",
    dotClassName: "bg-cat-blue",
  },
];

export default function Home() {
  return (
    <>
      <SiteHeader />
      <main className="flex-1">
        <div className="relative overflow-hidden">
          <div
            aria-hidden
            className="pointer-events-none absolute -top-24 left-1/2 h-72 w-[36rem] -translate-x-1/2 rounded-full bg-cat-blue/10 blur-3xl dark:bg-cat-blue/15"
          />
          <div
            aria-hidden
            className="pointer-events-none absolute top-10 right-1/4 h-56 w-56 rounded-full bg-cat-aqua/10 blur-3xl dark:bg-cat-aqua/15"
          />
          <Container className="relative max-w-3xl py-20 text-center">
            <h1 className="text-4xl font-semibold tracking-tight text-foreground sm:text-5xl">
              Agenda citas sin llamadas ni WhatsApp
            </h1>
            <p className="mx-auto mt-4 max-w-xl text-base text-muted">
              TuCita le da a tu negocio una página propia donde tus clientes ven tus
              horarios disponibles y reservan solos, en segundos.
            </p>
            <div className="mt-8 flex flex-col items-stretch justify-center gap-3 sm:flex-row sm:items-center">
              <Link
                href="/comenzar"
                className="rounded-md bg-accent px-5 py-2.5 text-center text-sm font-medium text-accent-foreground hover:bg-accent-hover"
              >
                Registrar mi negocio
              </Link>
              <Link
                href="/login"
                className="rounded-md border border-border bg-surface px-5 py-2.5 text-center text-sm font-medium text-foreground hover:bg-muted-background"
              >
                Ya tengo cuenta
              </Link>
            </div>
          </Container>
        </div>

        <Container className="max-w-4xl pb-20">
          <div className="grid gap-4 sm:grid-cols-3">
            {steps.map((step, index) => (
              <div key={step.title} className={`rounded-lg border p-6 shadow-sm ${step.theme}`}>
                <div className="flex items-center gap-2">
                  <span className={`h-2 w-2 rounded-full ${step.dotClassName}`} aria-hidden />
                  <span className="text-sm font-medium text-muted">
                    {String(index + 1).padStart(2, "0")}
                  </span>
                </div>
                <h2 className="mt-2 text-base font-semibold text-foreground">{step.title}</h2>
                <p className="mt-1 text-sm text-muted">{step.description}</p>
              </div>
            ))}
          </div>
        </Container>
      </main>
    </>
  );
}
