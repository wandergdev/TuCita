import Link from "next/link";
import { Container } from "@/components/ui/Container";

export function SiteHeader() {
  return (
    <header className="border-b border-border">
      <Container className="flex max-w-5xl items-center justify-between py-4">
        <Link href="/" className="text-lg font-semibold text-foreground">
          TuCita
        </Link>
        <nav className="flex items-center gap-4 text-sm">
          <Link href="/login" className="text-foreground hover:text-accent">
            Iniciar sesión
          </Link>
          <Link
            href="/registro"
            className="rounded-md bg-accent px-3 py-1.5 font-medium text-accent-foreground hover:bg-accent-hover"
          >
            Registrar mi negocio
          </Link>
        </nav>
      </Container>
    </header>
  );
}
