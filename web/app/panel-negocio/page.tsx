"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import type { ChangeEvent } from "react";
import { useRouter } from "next/navigation";
import Image from "next/image";
import { Container } from "@/components/ui/Container";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Alert } from "@/components/ui/Alert";
import { Badge } from "@/components/ui/Badge";
import { ThemeToggle } from "@/components/ui/ThemeToggle";
import { ShareLinkCard } from "@/components/dashboard/ShareLinkCard";
import { CATEGORY_LABELS } from "@/lib/labels";
import { SITE_DOMAIN } from "@/lib/site";
import { clearSession, getSession } from "@/lib/auth";
import {
  ApiError,
  addBusinessPhoto,
  createBusinessInvite,
  deleteBusinessPhoto,
  getBusinessPhotoUploadSignature,
  getMyBusinessPhotos,
  getMyRoster,
  removeProfessionalFromRoster,
  uploadToCloudinary,
} from "@/lib/api";
import type {
  AuthSession,
  BusinessPhoto,
  BusinessPhotoKind,
  PortfolioPhotoStatus,
  RosterProfessionalSummary,
} from "@/types/api";

const MAX_SPACE_PHOTOS = 4;

const PHOTO_STATUS_LABELS: Record<PortfolioPhotoStatus, string> = {
  Pending: "Pendiente",
  Approved: "Aprobada",
  Rejected: "Rechazada",
};

const PHOTO_STATUS_CLASSES: Record<PortfolioPhotoStatus, string> = {
  Pending: "bg-amber-50 text-amber-700 dark:bg-amber-500/15 dark:text-amber-300",
  Approved: "bg-emerald-50 text-emerald-700 dark:bg-emerald-500/15 dark:text-emerald-300",
  Rejected: "bg-rose-50 text-rose-700 dark:bg-rose-500/15 dark:text-rose-300",
};

export default function PanelNegocioPage() {
  const router = useRouter();
  const [session, setSessionState] = useState<AuthSession | undefined>(undefined);
  const [roster, setRoster] = useState<RosterProfessionalSummary[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [inviteUrl, setInviteUrl] = useState<string | null>(null);
  const [isCreatingInvite, setIsCreatingInvite] = useState(false);
  const [copied, setCopied] = useState(false);
  const [removingId, setRemovingId] = useState<string | null>(null);

  useEffect(() => {
    const current = getSession();
    if (!current || current.accountType !== "business") {
      router.replace("/login");
      return;
    }
    setSessionState(current);
  }, [router]);

  const loadRoster = useCallback(() => {
    if (!session || session.accountType !== "business") return;

    getMyRoster(session.token)
      .then(setRoster)
      .catch((err) =>
        setError(err instanceof ApiError ? err.message : "No se pudieron cargar tus profesionales."),
      );
  }, [session]);

  useEffect(() => {
    loadRoster();
  }, [loadRoster]);

  async function handleRemoveProfessional(professionalId: string) {
    if (!session || session.accountType !== "business") return;
    setError(null);
    setRemovingId(professionalId);
    try {
      await removeProfessionalFromRoster(session.token, professionalId);
      loadRoster();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo quitar al profesional.");
    } finally {
      setRemovingId(null);
    }
  }

  function handleLogout() {
    clearSession();
    router.push("/login");
  }

  async function handleCreateInvite() {
    if (!session || session.accountType !== "business") return;
    setError(null);
    setCopied(false);
    setIsCreatingInvite(true);
    try {
      const url = await createBusinessInvite(session.token);
      setInviteUrl(url);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo generar la invitación.");
    } finally {
      setIsCreatingInvite(false);
    }
  }

  async function handleCopyInvite() {
    if (!inviteUrl) return;
    await navigator.clipboard.writeText(inviteUrl);
    setCopied(true);
  }

  if (session === undefined || session.accountType !== "business") {
    return null;
  }

  return (
    <main className="flex-1 py-10">
      <Container className="max-w-3xl">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h1 className="text-2xl font-semibold text-foreground">{session.name}</h1>
            <p className="mt-1 text-sm text-muted">
              {SITE_DOMAIN}/{session.slug}
            </p>
          </div>
          <div className="flex items-center gap-2 self-end sm:self-auto">
            <ThemeToggle />
            <Button variant="outline" onClick={handleLogout}>
              Cerrar sesión
            </Button>
          </div>
        </div>

        {error && (
          <Alert tone="error" className="mt-6">
            {error}
          </Alert>
        )}

        <Card className="mt-6 flex flex-col gap-4">
          <div>
            <h2 className="text-lg font-semibold text-foreground">Invitar profesional</h2>
            <p className="mt-1 text-sm text-muted">
              Genera un enlace de un solo uso para que un profesional se registre ya asociado a tu
              negocio.
            </p>
          </div>
          <Button
            type="button"
            disabled={isCreatingInvite}
            onClick={handleCreateInvite}
            className="self-start"
          >
            {isCreatingInvite ? "Generando…" : "Generar enlace de invitación"}
          </Button>
          {inviteUrl && (
            <div className="flex flex-col gap-2 rounded-md border border-border p-3 sm:flex-row sm:items-center sm:justify-between">
              <code className="break-all text-sm text-foreground">{inviteUrl}</code>
              <Button type="button" variant="outline" onClick={handleCopyInvite}>
                {copied ? "¡Copiado!" : "Copiar"}
              </Button>
            </div>
          )}
        </Card>

        <Card className="mt-6 flex flex-col gap-4">
          <h2 className="text-lg font-semibold text-foreground">Tus profesionales</h2>

          {roster === null && <p className="text-sm text-muted">Cargando…</p>}
          {roster !== null && roster.length === 0 && (
            <p className="text-sm text-muted">
              Todavía no tienes profesionales. Invita al primero con el enlace de arriba.
            </p>
          )}

          {roster && roster.length > 0 && (
            <div className="flex flex-col gap-3">
              {roster.map((professional) => (
                <div
                  key={professional.professionalId}
                  className="flex flex-col gap-3 rounded-md border border-border p-3 sm:flex-row sm:items-center sm:justify-between"
                >
                  <div>
                    <p className="text-sm font-medium text-foreground">{professional.fullName}</p>
                    <p className="text-sm text-muted">
                      {SITE_DOMAIN}/{professional.slug}
                    </p>
                  </div>
                  <div className="flex items-center gap-2 self-end sm:self-auto">
                    <Badge className="bg-accent/10 text-accent">
                      {CATEGORY_LABELS[professional.category]}
                    </Badge>
                    <Button
                      type="button"
                      variant="outline"
                      disabled={removingId === professional.professionalId}
                      onClick={() => handleRemoveProfessional(professional.professionalId)}
                    >
                      {removingId === professional.professionalId ? "Quitando…" : "Desvincular"}
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </Card>

        <BusinessPhotosSection token={session.token} />
      </Container>

      <ShareLinkCard slug={session.slug} />
    </main>
  );
}

function BusinessPhotosSection({ token }: { token: string }) {
  const [photos, setPhotos] = useState<BusinessPhoto[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [busyId, setBusyId] = useState<string | null>(null);
  const coverFileInputRef = useRef<HTMLInputElement>(null);
  const spaceFileInputRef = useRef<HTMLInputElement>(null);

  const load = useCallback(() => {
    getMyBusinessPhotos(token)
      .then(setPhotos)
      .catch((err) =>
        setError(err instanceof ApiError ? err.message : "No se pudieron cargar las fotos."),
      );
  }, [token]);

  useEffect(() => {
    load();
  }, [load]);

  async function handleUpload(kind: BusinessPhotoKind, file: File) {
    setError(null);
    setIsUploading(true);
    try {
      const signature = await getBusinessPhotoUploadSignature(token);
      const { imageUrl, publicId } = await uploadToCloudinary(signature, file);
      await addBusinessPhoto(token, kind, imageUrl, publicId);
      load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo subir la foto.");
    } finally {
      setIsUploading(false);
    }
  }

  async function handleDelete(id: string) {
    setBusyId(id);
    setError(null);
    try {
      await deleteBusinessPhoto(token, id);
      load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo eliminar la foto.");
    } finally {
      setBusyId(null);
    }
  }

  function handleCoverFileChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (file) handleUpload("Cover", file);
  }

  function handleSpaceFileChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (file) handleUpload("Space", file);
  }

  const coverPhoto = photos?.find((p) => p.kind === "Cover") ?? null;
  const spacePhotos = photos?.filter((p) => p.kind === "Space") ?? [];
  const spaceAtLimit = spacePhotos.length >= MAX_SPACE_PHOTOS;

  return (
    <>
      {error && (
        <Alert tone="error" className="mt-6">
          {error}
        </Alert>
      )}

      <Card className="mt-6 flex flex-col gap-4">
        <div>
          <h2 className="text-lg font-semibold text-foreground">Imagen de portada</h2>
          <p className="mt-1 text-sm text-muted">
            Se muestra de fondo en el encabezado de tu página pública. Se revisa antes de
            publicarse.
          </p>
        </div>

        <input
          ref={coverFileInputRef}
          type="file"
          accept="image/*"
          className="hidden"
          onChange={handleCoverFileChange}
        />

        {photos === null && <p className="text-sm text-muted">Cargando…</p>}

        {photos !== null && !coverPhoto && (
          <Button
            type="button"
            disabled={isUploading}
            onClick={() => coverFileInputRef.current?.click()}
            className="self-start"
          >
            {isUploading ? "Subiendo…" : "Subir imagen de portada"}
          </Button>
        )}

        {coverPhoto && (
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
            <div className="relative aspect-video w-full overflow-hidden rounded-md bg-muted-background sm:w-64">
              <Image
                src={coverPhoto.imageUrl}
                alt="Portada"
                fill
                className="object-cover"
                sizes="256px"
              />
            </div>
            <div className="flex flex-col gap-2">
              <Badge className={PHOTO_STATUS_CLASSES[coverPhoto.status]}>
                {PHOTO_STATUS_LABELS[coverPhoto.status]}
              </Badge>
              <Button
                type="button"
                variant="outline"
                disabled={busyId === coverPhoto.id}
                onClick={() => handleDelete(coverPhoto.id)}
                className="self-start"
              >
                Eliminar
              </Button>
            </div>
          </div>
        )}
      </Card>

      <Card className="mt-6 flex flex-col gap-4">
        <div>
          <h2 className="text-lg font-semibold text-foreground">Fotos del espacio</h2>
          <p className="mt-1 text-sm text-muted">
            Hasta {MAX_SPACE_PHOTOS} fotos de tu local (interior o exterior). Se revisan antes de
            publicarse.
          </p>
        </div>

        <input
          ref={spaceFileInputRef}
          type="file"
          accept="image/*"
          className="hidden"
          onChange={handleSpaceFileChange}
        />

        <Button
          type="button"
          disabled={isUploading || spaceAtLimit}
          onClick={() => spaceFileInputRef.current?.click()}
          className="self-start"
        >
          {isUploading ? "Subiendo…" : spaceAtLimit ? "Alcanzaste el límite de fotos" : "Subir foto"}
        </Button>

        {photos !== null && spacePhotos.length === 0 && (
          <p className="text-sm text-muted">Todavía no has subido fotos del espacio.</p>
        )}

        {spacePhotos.length > 0 && (
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
            {spacePhotos.map((photo) => (
              <div key={photo.id} className="flex flex-col gap-2 rounded-md border border-border p-2">
                <div className="relative aspect-square overflow-hidden rounded-md bg-muted-background">
                  <Image
                    src={photo.imageUrl}
                    alt="Foto del espacio"
                    fill
                    className="object-cover"
                    sizes="150px"
                  />
                </div>
                <Badge className={PHOTO_STATUS_CLASSES[photo.status]}>
                  {PHOTO_STATUS_LABELS[photo.status]}
                </Badge>
                <Button
                  type="button"
                  variant="outline"
                  disabled={busyId === photo.id}
                  onClick={() => handleDelete(photo.id)}
                >
                  Eliminar
                </Button>
              </div>
            ))}
          </div>
        )}
      </Card>
    </>
  );
}
