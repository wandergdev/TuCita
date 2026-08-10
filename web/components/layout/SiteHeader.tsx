import Link from "next/link";
import { Container } from "@/components/ui/Container";
import { ThemeToggle } from "@/components/ui/ThemeToggle";

export function SiteHeader() {
  return (
    <header className="border-b border-border">
      <Container className="flex max-w-5xl flex-wrap items-center justify-between gap-3 py-4">
        <Link href="/" className="text-lg font-semibold text-foreground">
          TuCita
        </Link>
        <nav className="flex items-center gap-2 text-sm sm:gap-4">
          <Link href="/login" className="text-foreground hover:text-accent">
            Iniciar sesión
          </Link>
          <Link
            href="/comenzar"
            className="rounded-md bg-accent px-2.5 py-1.5 font-medium text-accent-foreground hover:bg-accent-hover sm:px-3"
          >
            <span className="hidden sm:inline">Registrar mi negocio</span>
            <span className="sm:hidden">Registrar</span>
          </Link>
          <ThemeToggle />
        </nav>
      </Container>
    </header>
  );
}
