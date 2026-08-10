"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import type { ChangeEvent } from "react";
import { useRouter } from "next/navigation";
import Image from "next/image";
import { Container } from "@/components/ui/Container";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Alert } from "@/components/ui/Alert";
import { clearAdminSession, getAdminSession } from "@/lib/adminAuth";
import {
  ApiError,
  approvePendingBusinessPhoto,
  approvePendingPhoto,
  getAdminBusinessPhotoUploadSignature,
  getAdminPendingBusinessPhotos,
  getAdminPendingPhotos,
  getAdminUploadSignature,
  rejectPendingBusinessPhoto,
  rejectPendingPhoto,
  replaceBusinessPhotoImage,
  replacePortfolioPhotoImage,
  uploadToCloudinary,
} from "@/lib/api";
import type { AdminPendingBusinessPhoto, AdminPendingPhoto, BusinessPhotoKind } from "@/types/api";

function toDownloadUrl(imageUrl: string): string {
  return imageUrl.replace("/image/upload/", "/image/upload/fl_attachment/");
}

const BUSINESS_PHOTO_KIND_LABELS: Record<BusinessPhotoKind, string> = {
  Cover: "Portada",
  Space: "Espacio",
};

export default function AdminPortfolioPage() {
  const router = useRouter();
  const [token, setToken] = useState<string | null>(null);

  useEffect(() => {
    const session = getAdminSession();
    if (!session) {
      router.replace("/admin/login");
      return;
    }
    setToken(session.token);
  }, [router]);

  function handleLogout() {
    clearAdminSession();
    router.push("/admin/login");
  }

  if (!token) return null;

  return (
    <main className="flex-1 py-10">
      <Container className="max-w-4xl">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-semibold text-foreground">Fotos pendientes</h1>
            <p className="mt-1 text-sm text-muted">
              Revísalas, edítalas si hace falta, y apruébalas o recházalas.
            </p>
          </div>
          <Button variant="outline" onClick={handleLogout}>
            Cerrar sesión
          </Button>
        </div>

        <div className="mt-6">
          <PendingPhotosList token={token} />
        </div>

        <div className="mt-10">
          <h2 className="text-xl font-semibold text-foreground">Fotos de negocios pendientes</h2>
          <p className="mt-1 text-sm text-muted">Portadas y fotos del espacio de cada negocio.</p>
          <div className="mt-4">
            <PendingBusinessPhotosList token={token} />
          </div>
        </div>
      </Container>
    </main>
  );
}

function PendingPhotosList({ token }: { token: string }) {
  const [photos, setPhotos] = useState<AdminPendingPhoto[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(() => {
    getAdminPendingPhotos(token)
      .then(setPhotos)
      .catch((err) =>
        setError(err instanceof ApiError ? err.message : "No se pudieron cargar las fotos pendientes."),
      );
  }, [token]);

  useEffect(() => {
    load();
  }, [load]);

  if (error) return <Alert tone="error">{error}</Alert>;
  if (photos === null) return <p className="text-sm text-muted">Cargando…</p>;
  if (photos.length === 0) return <p className="text-sm text-muted">No hay fotos pendientes.</p>;

  return (
    <div className="flex flex-col gap-4">
      {photos.map((photo) => (
        <PendingPhotoCard key={photo.id} token={token} photo={photo} onChanged={load} />
      ))}
    </div>
  );
}

function PendingPhotoCard({
  token,
  photo,
  onChanged,
}: {
  token: string;
  photo: AdminPendingPhoto;
  onChanged: () => void;
}) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isBusy, setIsBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleReplace(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (!file) return;

    setError(null);
    setIsBusy(true);
    try {
      const signature = await getAdminUploadSignature(token, photo.id);
      const { imageUrl, publicId } = await uploadToCloudinary(signature, file);
      await replacePortfolioPhotoImage(token, photo.id, imageUrl, publicId);
      onChanged();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo subir la versión editada.");
      setIsBusy(false);
    }
  }

  async function handleApprove() {
    setError(null);
    setIsBusy(true);
    try {
      await approvePendingPhoto(token, photo.id);
      onChanged();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo aprobar la foto.");
      setIsBusy(false);
    }
  }

  async function handleReject() {
    setError(null);
    setIsBusy(true);
    try {
      await rejectPendingPhoto(token, photo.id);
      onChanged();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo rechazar la foto.");
      setIsBusy(false);
    }
  }

  return (
    <Card className="flex flex-col gap-4 sm:flex-row">
      <div className="relative aspect-square w-full shrink-0 overflow-hidden rounded-md bg-muted-background sm:w-56">
        <Image
          src={photo.imageUrl}
          alt={photo.caption ?? photo.professionalFullName}
          fill
          className="object-cover"
          sizes="(min-width: 640px) 224px, 100vw"
        />
      </div>

      <div className="flex flex-1 flex-col gap-3">
        <div>
          <p className="text-sm font-medium text-foreground">{photo.professionalFullName}</p>
          <p className="text-sm text-muted">/{photo.professionalSlug}</p>
          {photo.caption && <p className="mt-1 text-sm text-muted">“{photo.caption}”</p>}
        </div>

        {error && <Alert tone="error">{error}</Alert>}

        <div className="flex flex-wrap gap-2">
          <a href={toDownloadUrl(photo.imageUrl)} target="_blank" rel="noreferrer">
            <Button type="button" variant="outline" disabled={isBusy}>
              Descargar
            </Button>
          </a>

          <input
            ref={fileInputRef}
            type="file"
            accept="image/*"
            className="hidden"
            onChange={handleReplace}
          />
          <Button
            type="button"
            variant="outline"
            disabled={isBusy}
            onClick={() => fileInputRef.current?.click()}
          >
            Subir versión editada
          </Button>

          <Button type="button" disabled={isBusy} onClick={handleApprove}>
            Aprobar
          </Button>
          <Button type="button" variant="danger" disabled={isBusy} onClick={handleReject}>
            Rechazar
          </Button>
        </div>
      </div>
    </Card>
  );
}

function PendingBusinessPhotosList({ token }: { token: string }) {
  const [photos, setPhotos] = useState<AdminPendingBusinessPhoto[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(() => {
    getAdminPendingBusinessPhotos(token)
      .then(setPhotos)
      .catch((err) =>
        setError(err instanceof ApiError ? err.message : "No se pudieron cargar las fotos pendientes."),
      );
  }, [token]);

  useEffect(() => {
    load();
  }, [load]);

  if (error) return <Alert tone="error">{error}</Alert>;
  if (photos === null) return <p className="text-sm text-muted">Cargando…</p>;
  if (photos.length === 0) return <p className="text-sm text-muted">No hay fotos pendientes.</p>;

  return (
    <div className="flex flex-col gap-4">
      {photos.map((photo) => (
        <PendingBusinessPhotoCard key={photo.id} token={token} photo={photo} onChanged={load} />
      ))}
    </div>
  );
}

function PendingBusinessPhotoCard({
  token,
  photo,
  onChanged,
}: {
  token: string;
  photo: AdminPendingBusinessPhoto;
  onChanged: () => void;
}) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isBusy, setIsBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleReplace(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (!file) return;

    setError(null);
    setIsBusy(true);
    try {
      const signature = await getAdminBusinessPhotoUploadSignature(token, photo.id);
      const { imageUrl, publicId } = await uploadToCloudinary(signature, file);
      await replaceBusinessPhotoImage(token, photo.id, imageUrl, publicId);
      onChanged();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo subir la versión editada.");
      setIsBusy(false);
    }
  }

  async function handleApprove() {
    setError(null);
    setIsBusy(true);
    try {
      await approvePendingBusinessPhoto(token, photo.id);
      onChanged();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo aprobar la foto.");
      setIsBusy(false);
    }
  }

  async function handleReject() {
    setError(null);
    setIsBusy(true);
    try {
      await rejectPendingBusinessPhoto(token, photo.id);
      onChanged();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo rechazar la foto.");
      setIsBusy(false);
    }
  }

  return (
    <Card className="flex flex-col gap-4 sm:flex-row">
      <div className="relative aspect-square w-full shrink-0 overflow-hidden rounded-md bg-muted-background sm:w-56">
        <Image
          src={photo.imageUrl}
          alt={photo.caption ?? photo.businessName}
          fill
          className="object-cover"
          sizes="(min-width: 640px) 224px, 100vw"
        />
      </div>

      <div className="flex flex-1 flex-col gap-3">
        <div>
          <p className="text-sm font-medium text-foreground">
            {photo.businessName} · {BUSINESS_PHOTO_KIND_LABELS[photo.kind]}
          </p>
          <p className="text-sm text-muted">/{photo.businessSlug}</p>
          {photo.caption && <p className="mt-1 text-sm text-muted">“{photo.caption}”</p>}
        </div>

        {error && <Alert tone="error">{error}</Alert>}

        <div className="flex flex-wrap gap-2">
          <a href={toDownloadUrl(photo.imageUrl)} target="_blank" rel="noreferrer">
            <Button type="button" variant="outline" disabled={isBusy}>
              Descargar
            </Button>
          </a>

          <input
            ref={fileInputRef}
            type="file"
            accept="image/*"
            className="hidden"
            onChange={handleReplace}
          />
          <Button
            type="button"
            variant="outline"
            disabled={isBusy}
            onClick={() => fileInputRef.current?.click()}
          >
            Subir versión editada
          </Button>

          <Button type="button" disabled={isBusy} onClick={handleApprove}>
            Aprobar
          </Button>
          <Button type="button" variant="danger" disabled={isBusy} onClick={handleReject}>
            Rechazar
          </Button>
        </div>
      </div>
    </Card>
  );
}
