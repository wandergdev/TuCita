import Link from "next/link";
import { SiteHeader } from "@/components/layout/SiteHeader";
import { Container } from "@/components/ui/Container";

export default function NotFound() {
  return (
    <>
      <SiteHeader />
      <main className="flex flex-1 items-center justify-center py-20">
        <Container className="max-w-md text-center">
          <h1 className="text-2xl font-semibold text-foreground">Página no encontrada</h1>
          <p className="mt-2 text-sm text-muted">
            El profesional que buscas no existe o su enlace ya no está disponible.
          </p>
          <Link
            href="/"
            className="mt-6 inline-block text-sm font-medium text-accent hover:text-accent-hover"
          >
            Volver al inicio
          </Link>
        </Container>
      </main>
    </>
  );
}
