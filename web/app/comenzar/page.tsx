import Link from "next/link";
import { SiteHeader } from "@/components/layout/SiteHeader";
import { Container } from "@/components/ui/Container";
import { Card } from "@/components/ui/Card";

export default function ComenzarPage() {
  return (
    <>
      <SiteHeader />
      <main className="flex flex-1 items-center justify-center py-12">
        <Container className="max-w-2xl">
          <h1 className="text-center text-2xl font-semibold text-foreground">
            ¿Cómo quieres registrarte?
          </h1>
          <p className="mt-1 text-center text-sm text-muted">
            Elige la opción que mejor describa tu negocio.
          </p>

          <div className="mt-8 grid gap-4 sm:grid-cols-2">
            <Link href="/registro">
              <Card className="flex h-full flex-col gap-2 transition-colors hover:border-accent">
                <h2 className="text-lg font-semibold text-foreground">Profesional independiente</h2>
                <p className="text-sm text-muted">
                  Trabajas por tu cuenta (barbero, estilista, dentista, psicólogo/a) y quieres tu
                  propia página de reservas.
                </p>
              </Card>
            </Link>

            <Link href="/registro-negocio">
              <Card className="flex h-full flex-col gap-2 transition-colors hover:border-accent">
                <h2 className="text-lg font-semibold text-foreground">Negocio con varios profesionales</h2>
                <p className="text-sm text-muted">
                  Tienes una peluquería, clínica u otro negocio con varios profesionales, y quieres
                  invitarlos a todos.
                </p>
              </Card>
            </Link>
          </div>
        </Container>
      </main>
    </>
  );
}
