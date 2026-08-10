import Link from "next/link";
import Image from "next/image";
import { notFound } from "next/navigation";
import { SiteHeader } from "@/components/layout/SiteHeader";
import { Container } from "@/components/ui/Container";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { BookingWidget } from "@/components/booking/BookingWidget";
import { SpacePhotoGallery } from "@/components/business/SpacePhotoGallery";
import { CATEGORY_LABELS, CATEGORY_TAGLINES, DEFAULT_HERO_TAGLINE } from "@/lib/labels";
import { getBusinessProfile, getProfessionalProfile } from "@/lib/api";

interface ProfessionalPageProps {
  params: Promise<{ slug: string }>;
}

export default async function ProfessionalPage({ params }: ProfessionalPageProps) {
  const { slug } = await params;
  // En esta versión de Next.js el segmento dinámico puede llegar todavía
  // percent-encoded (ej. "%C3%B3" en vez de "ó") cuando contiene caracteres
  // no ASCII, y volver a codificarlo en la llamada a la API lo duplica.
  // decodeURIComponent es un no-op seguro sobre slugs que ya vienen limpios.
  const decodedSlug = decodeURIComponent(slug);
  const profile = await getProfessionalProfile(decodedSlug);

  if (profile) {
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

            {profile.photos.length > 0 && (
              <div className="mt-10">
                <h2 className="text-lg font-semibold text-foreground">Trabajos recientes</h2>
                <div className="mt-3 grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4">
                  {profile.photos.map((photo, index) => (
                    <div
                      key={photo.imageUrl}
                      className="relative aspect-square overflow-hidden rounded-md bg-muted-background"
                    >
                      <Image
                        src={photo.imageUrl}
                        alt={photo.caption ?? `Foto ${index + 1} de ${profile.fullName}`}
                        fill
                        className="object-cover"
                        sizes="(min-width: 768px) 25vw, (min-width: 640px) 33vw, 50vw"
                      />
                    </div>
                  ))}
                </div>
              </div>
            )}
          </Container>
        </main>
      </>
    );
  }

  const business = await getBusinessProfile(decodedSlug);

  if (!business) {
    notFound();
  }

  const heroPhotos = business.professionals.flatMap((p) => p.coverPhotoUrls).slice(0, 4);
  const heroTagline = business.professionals[0]
    ? CATEGORY_TAGLINES[business.professionals[0].category]
    : DEFAULT_HERO_TAGLINE;

  return (
    <>
      <SiteHeader />
      <main className="flex-1">
        {business.coverImageUrl ? (
          <div className="relative h-64 w-full overflow-hidden sm:h-80">
            <Image
              src={business.coverImageUrl}
              alt={business.name}
              fill
              priority
              className="object-cover"
              sizes="100vw"
            />
            <div className="absolute inset-0 bg-black/55" />
            <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-transparent to-transparent" />
            <div className="absolute inset-0 flex flex-col items-center justify-center px-4 text-center">
              <h1 className="text-3xl font-semibold tracking-tight text-white drop-shadow-sm sm:text-5xl">
                {business.name}
              </h1>
              <p className="mx-auto mt-3 max-w-xl text-base text-white/90 drop-shadow-sm">
                {heroTagline}
              </p>
            </div>
          </div>
        ) : (
          <div className="relative overflow-hidden">
            <div
              aria-hidden
              className="pointer-events-none absolute -top-24 left-1/2 h-72 w-[36rem] -translate-x-1/2 rounded-full bg-cat-blue/10 blur-3xl dark:bg-cat-blue/15"
            />
            <div
              aria-hidden
              className="pointer-events-none absolute top-10 right-1/4 h-56 w-56 rounded-full bg-cat-aqua/10 blur-3xl dark:bg-cat-aqua/15"
            />
            <Container className="relative max-w-4xl py-16 text-center">
              <h1 className="text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">
                {business.name}
              </h1>
              <p className="mx-auto mt-3 max-w-xl text-base text-muted">{heroTagline}</p>

              {heroPhotos.length > 0 && (
                <div className="mx-auto mt-8 flex max-w-md justify-center gap-3">
                  {heroPhotos.map((url) => (
                    <div
                      key={url}
                      className="relative aspect-square w-full overflow-hidden rounded-xl shadow-sm"
                    >
                      <Image
                        src={url}
                        alt={business.name}
                        fill
                        className="object-cover"
                        sizes="120px"
                      />
                    </div>
                  ))}
                </div>
              )}
            </Container>
          </div>
        )}

        <Container className="max-w-4xl pb-16">
          <h2 className="text-lg font-semibold text-foreground">Nuestro equipo</h2>

          {business.professionals.length === 0 ? (
            <p className="mt-3 text-sm text-muted">
              Este negocio todavía no tiene profesionales disponibles.
            </p>
          ) : (
            <div className="mt-4 grid gap-4 sm:grid-cols-2">
              {business.professionals.map((professional) => (
                <Link key={professional.slug} href={`/${professional.slug}`}>
                  <Card className="flex h-full flex-col gap-2 overflow-hidden transition-colors hover:border-accent">
                    {professional.coverPhotoUrls.length > 0 && (
                      <div
                        className={`relative -mx-6 -mt-6 grid gap-0.5 overflow-hidden bg-muted-background ${
                          professional.coverPhotoUrls.length > 1 ? "grid-cols-2" : "grid-cols-1"
                        }`}
                      >
                        {professional.coverPhotoUrls.map((url) => (
                          <div key={url} className="relative aspect-square">
                            <Image
                              src={url}
                              alt={professional.fullName}
                              fill
                              className="object-cover"
                              sizes="(min-width: 640px) 25vw, 50vw"
                            />
                          </div>
                        ))}
                      </div>
                    )}
                    <Badge className="w-fit bg-accent/10 text-accent">
                      {CATEGORY_LABELS[professional.category]}
                    </Badge>
                    <h3 className="text-lg font-semibold text-foreground">{professional.fullName}</h3>
                    <span className="text-sm font-medium text-accent">Ver disponibilidad →</span>
                  </Card>
                </Link>
              ))}
            </div>
          )}

          {business.spacePhotos.length > 0 && (
            <div className="mt-10">
              <h2 className="text-lg font-semibold text-foreground">Nuestro espacio</h2>
              <div className="mt-4">
                <SpacePhotoGallery photos={business.spacePhotos} altPrefix={business.name} />
              </div>
            </div>
          )}
        </Container>
      </main>
    </>
  );
}
