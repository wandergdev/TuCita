import { notFound } from "next/navigation";
import { SiteHeader } from "@/components/layout/SiteHeader";
import { Container } from "@/components/ui/Container";
import { BookingWidget } from "@/components/booking/BookingWidget";
import { CATEGORY_LABELS } from "@/lib/labels";
import { getProfessionalProfile } from "@/lib/api";

interface ProfessionalPageProps {
  params: Promise<{ slug: string }>;
}

export default async function ProfessionalPage({ params }: ProfessionalPageProps) {
  const { slug } = await params;
  // En esta versión de Next.js el segmento dinámico puede llegar todavía
  // percent-encoded (ej. "%C3%B3" en vez de "ó") cuando contiene caracteres
  // no ASCII, y volver a codificarlo en la llamada a la API lo duplica.
  // decodeURIComponent es un no-op seguro sobre slugs que ya vienen limpios.
  const profile = await getProfessionalProfile(decodeURIComponent(slug));

  if (!profile) {
    notFound();
  }

  return (
    <>
      <SiteHeader />
      <main className="flex-1 py-12">
        <Container>
          <div className="mb-6">
            <p className="text-sm font-medium text-accent">{CATEGORY_LABELS[profile.category]}</p>
            <h1 className="text-2xl font-semibold text-foreground">{profile.fullName}</h1>
            <p className="mt-1 text-sm text-muted">
              Citas de {profile.appointmentDurationMinutes} minutos.
            </p>
          </div>
          <BookingWidget slug={profile.slug} />
        </Container>
      </main>
    </>
  );
}
