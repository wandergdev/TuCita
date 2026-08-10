"use client";

import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import type { ChangeEvent, ReactNode } from "react";
import { useRouter } from "next/navigation";
import Image from "next/image";
import { Container } from "@/components/ui/Container";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Alert } from "@/components/ui/Alert";
import { Badge } from "@/components/ui/Badge";
import { clearAdminSession, getAdminSession } from "@/lib/adminAuth";
import { CATEGORY_LABELS, DEFAULT_HERO_TAGLINE } from "@/lib/labels";
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

function PreviewTile({ label, note, children }: { label: string; note?: string; children: ReactNode }) {
  return (
    <div className="flex flex-col gap-2">
      <p className="text-xs font-medium text-muted">{label}</p>
      {children}
      {note && <p className="max-w-[14rem] text-[11px] text-muted">{note}</p>}
    </div>
  );
}

function SelectCheckbox({
  checked,
  disabled,
  onChange,
}: {
  checked: boolean;
  disabled: boolean;
  onChange: () => void;
}) {
  return (
    <label className="flex items-center gap-2 text-xs text-muted">
      <input
        type="checkbox"
        className="h-4 w-4 accent-accent disabled:opacity-40"
        checked={checked}
        disabled={disabled}
        onChange={onChange}
      />
      Seleccionar
    </label>
  );
}

function BulkApproveBar({
  count,
  isBusy,
  onCancel,
  onApprove,
}: {
  count: number;
  isBusy: boolean;
  onCancel: () => void;
  onApprove: () => void;
}) {
  return (
    <div className="sticky top-2 z-10 flex flex-wrap items-center justify-between gap-3 rounded-md border border-accent/30 bg-accent/5 p-3">
      <p className="text-sm text-foreground">
        {count} foto{count > 1 ? "s" : ""} seleccionada{count > 1 ? "s" : ""} (mismo dueño)
      </p>
      <div className="flex items-center gap-2">
        <Button type="button" variant="outline" disabled={isBusy} onClick={onCancel}>
          Cancelar
        </Button>
        <Button type="button" disabled={isBusy} onClick={onApprove}>
          {isBusy ? "Aprobando…" : `Aprobar ${count} seleccionadas`}
        </Button>
      </div>
    </div>
  );
}

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
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [isBulkApproving, setIsBulkApproving] = useState(false);
  const [bulkError, setBulkError] = useState<string | null>(null);

  const load = useCallback(() => {
    getAdminPendingPhotos(token)
      .then((result) => {
        setPhotos(result);
        const validIds = new Set(result.map((p) => p.id));
        setSelectedIds((prev) => new Set([...prev].filter((id) => validIds.has(id))));
      })
      .catch((err) =>
        setError(err instanceof ApiError ? err.message : "No se pudieron cargar las fotos pendientes."),
      );
  }, [token]);

  useEffect(() => {
    load();
  }, [load]);

  function toggleSelect(id: string) {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }

  const selectionOwnerId = useMemo(() => {
    if (!photos || selectedIds.size === 0) return null;
    return photos.find((p) => selectedIds.has(p.id))?.professionalId ?? null;
  }, [photos, selectedIds]);

  async function handleBulkApprove() {
    setBulkError(null);
    setIsBulkApproving(true);
    const ids = [...selectedIds];
    const results = await Promise.allSettled(ids.map((id) => approvePendingPhoto(token, id)));
    const failed = results.filter((r) => r.status === "rejected").length;
    if (failed > 0) {
      setBulkError(`No se pudieron aprobar ${failed} de ${ids.length} fotos.`);
    }
    setSelectedIds(new Set());
    setIsBulkApproving(false);
    load();
  }

  if (error) return <Alert tone="error">{error}</Alert>;
  if (photos === null) return <p className="text-sm text-muted">Cargando…</p>;
  if (photos.length === 0) return <p className="text-sm text-muted">No hay fotos pendientes.</p>;

  return (
    <div className="flex flex-col gap-4">
      {selectedIds.size > 0 && (
        <BulkApproveBar
          count={selectedIds.size}
          isBusy={isBulkApproving}
          onCancel={() => setSelectedIds(new Set())}
          onApprove={handleBulkApprove}
        />
      )}
      {bulkError && <Alert tone="error">{bulkError}</Alert>}
      {photos.map((photo) => (
        <PendingPhotoCard
          key={photo.id}
          token={token}
          photo={photo}
          onChanged={load}
          isSelected={selectedIds.has(photo.id)}
          selectionDisabled={selectionOwnerId !== null && selectionOwnerId !== photo.professionalId}
          onToggleSelect={() => toggleSelect(photo.id)}
        />
      ))}
    </div>
  );
}

function PendingPhotoCard({
  token,
  photo,
  onChanged,
  isSelected,
  selectionDisabled,
  onToggleSelect,
}: {
  token: string;
  photo: AdminPendingPhoto;
  onChanged: () => void;
  isSelected: boolean;
  selectionDisabled: boolean;
  onToggleSelect: () => void;
}) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isBusy, setIsBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showPreview, setShowPreview] = useState(false);

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
    <Card className="flex flex-col gap-4">
      <div className="flex flex-col gap-4 sm:flex-row">
        <div className="flex shrink-0 flex-col gap-2 sm:w-56">
          <SelectCheckbox checked={isSelected} disabled={selectionDisabled} onChange={onToggleSelect} />
          <div className="relative aspect-square w-full overflow-hidden rounded-md bg-muted-background">
            <Image
              src={photo.imageUrl}
              alt={photo.caption ?? photo.professionalFullName}
              fill
              className="object-cover"
              sizes="(min-width: 640px) 224px, 100vw"
            />
          </div>
        </div>

        <div className="flex flex-1 flex-col gap-3">
          <div>
            <p className="text-sm font-medium text-foreground">{photo.professionalFullName}</p>
            <p className="text-sm text-muted">/{photo.professionalSlug}</p>
            {photo.businessName && (
              <p className="text-sm text-muted">Negocio: {photo.businessName}</p>
            )}
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

            <Button
              type="button"
              variant="outline"
              disabled={isBusy}
              onClick={() => setShowPreview((value) => !value)}
            >
              {showPreview ? "Ocultar vista previa" : "Vista previa"}
            </Button>

            <Button type="button" disabled={isBusy} onClick={handleApprove}>
              Aprobar
            </Button>
            <Button type="button" variant="danger" disabled={isBusy} onClick={handleReject}>
              Rechazar
            </Button>
          </div>
        </div>
      </div>

      {showPreview && (
        <div className="flex flex-wrap gap-6 border-t border-border pt-4">
          <PreviewTile label={`En su página (/${photo.professionalSlug})`}>
            <div className="relative aspect-square w-32 overflow-hidden rounded-md bg-muted-background">
              <Image src={photo.imageUrl} alt="" fill className="object-cover" sizes="128px" />
            </div>
          </PreviewTile>

          {photo.businessName && photo.businessSlug && (
            <PreviewTile
              label={`En la tarjeta de "${photo.businessName}"`}
              note="Aproximado: si aprueba más de una foto, se mostrarán una junto a la otra."
            >
              <div className="w-40 overflow-hidden rounded-lg border border-border bg-surface p-3 shadow-sm">
                <div className="relative -mx-3 -mt-3 aspect-square overflow-hidden bg-muted-background">
                  <Image src={photo.imageUrl} alt="" fill className="object-cover" sizes="160px" />
                </div>
                <Badge className="mt-2 w-fit bg-accent/10 text-xs text-accent">
                  {CATEGORY_LABELS[photo.professionalCategory]}
                </Badge>
                <p className="mt-1 truncate text-sm font-semibold text-foreground">
                  {photo.professionalFullName}
                </p>
                <span className="text-xs font-medium text-accent">Ver disponibilidad →</span>
              </div>
            </PreviewTile>
          )}
        </div>
      )}
    </Card>
  );
}

function PendingBusinessPhotosList({ token }: { token: string }) {
  const [photos, setPhotos] = useState<AdminPendingBusinessPhoto[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [isBulkApproving, setIsBulkApproving] = useState(false);
  const [bulkError, setBulkError] = useState<string | null>(null);

  const load = useCallback(() => {
    getAdminPendingBusinessPhotos(token)
      .then((result) => {
        setPhotos(result);
        const validIds = new Set(result.map((p) => p.id));
        setSelectedIds((prev) => new Set([...prev].filter((id) => validIds.has(id))));
      })
      .catch((err) =>
        setError(err instanceof ApiError ? err.message : "No se pudieron cargar las fotos pendientes."),
      );
  }, [token]);

  useEffect(() => {
    load();
  }, [load]);

  function toggleSelect(id: string) {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }

  const selectionOwnerId = useMemo(() => {
    if (!photos || selectedIds.size === 0) return null;
    return photos.find((p) => selectedIds.has(p.id))?.businessId ?? null;
  }, [photos, selectedIds]);

  async function handleBulkApprove() {
    setBulkError(null);
    setIsBulkApproving(true);
    const ids = [...selectedIds];
    const results = await Promise.allSettled(ids.map((id) => approvePendingBusinessPhoto(token, id)));
    const failed = results.filter((r) => r.status === "rejected").length;
    if (failed > 0) {
      setBulkError(`No se pudieron aprobar ${failed} de ${ids.length} fotos.`);
    }
    setSelectedIds(new Set());
    setIsBulkApproving(false);
    load();
  }

  if (error) return <Alert tone="error">{error}</Alert>;
  if (photos === null) return <p className="text-sm text-muted">Cargando…</p>;
  if (photos.length === 0) return <p className="text-sm text-muted">No hay fotos pendientes.</p>;

  return (
    <div className="flex flex-col gap-4">
      {selectedIds.size > 0 && (
        <BulkApproveBar
          count={selectedIds.size}
          isBusy={isBulkApproving}
          onCancel={() => setSelectedIds(new Set())}
          onApprove={handleBulkApprove}
        />
      )}
      {bulkError && <Alert tone="error">{bulkError}</Alert>}
      {photos.map((photo) => (
        <PendingBusinessPhotoCard
          key={photo.id}
          token={token}
          photo={photo}
          onChanged={load}
          isSelected={selectedIds.has(photo.id)}
          selectionDisabled={selectionOwnerId !== null && selectionOwnerId !== photo.businessId}
          onToggleSelect={() => toggleSelect(photo.id)}
        />
      ))}
    </div>
  );
}

function PendingBusinessPhotoCard({
  token,
  photo,
  onChanged,
  isSelected,
  selectionDisabled,
  onToggleSelect,
}: {
  token: string;
  photo: AdminPendingBusinessPhoto;
  onChanged: () => void;
  isSelected: boolean;
  selectionDisabled: boolean;
  onToggleSelect: () => void;
}) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isBusy, setIsBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showPreview, setShowPreview] = useState(false);

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
    <Card className="flex flex-col gap-4">
      <div className="flex flex-col gap-4 sm:flex-row">
        <div className="flex shrink-0 flex-col gap-2 sm:w-56">
          <SelectCheckbox checked={isSelected} disabled={selectionDisabled} onChange={onToggleSelect} />
          <div className="relative aspect-square w-full overflow-hidden rounded-md bg-muted-background">
            <Image
              src={photo.imageUrl}
              alt={photo.caption ?? photo.businessName}
              fill
              className="object-cover"
              sizes="(min-width: 640px) 224px, 100vw"
            />
          </div>
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

            <Button
              type="button"
              variant="outline"
              disabled={isBusy}
              onClick={() => setShowPreview((value) => !value)}
            >
              {showPreview ? "Ocultar vista previa" : "Vista previa"}
            </Button>

            <Button type="button" disabled={isBusy} onClick={handleApprove}>
              Aprobar
            </Button>
            <Button type="button" variant="danger" disabled={isBusy} onClick={handleReject}>
              Rechazar
            </Button>
          </div>
        </div>
      </div>

      {showPreview && (
        <div className="border-t border-border pt-4">
          {photo.kind === "Cover" ? (
            <PreviewTile
              label={`Encabezado de "${photo.businessName}" (/${photo.businessSlug})`}
              note="El texto exacto varía según la categoría del equipo; esto es una aproximación."
            >
              <div className="relative h-28 w-56 overflow-hidden rounded-md">
                <Image src={photo.imageUrl} alt="" fill className="object-cover" sizes="224px" />
                <div className="absolute inset-0 bg-black/55" />
                <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-transparent to-transparent" />
                <div className="absolute inset-0 flex flex-col items-center justify-center px-3 text-center">
                  <p className="text-sm font-semibold text-white drop-shadow-sm">{photo.businessName}</p>
                  <p className="mt-1 line-clamp-2 text-[10px] text-white/90 drop-shadow-sm">
                    {DEFAULT_HERO_TAGLINE}
                  </p>
                </div>
              </div>
            </PreviewTile>
          ) : (
            <PreviewTile label={`En "Nuestro espacio" de "${photo.businessName}"`}>
              <div className="relative aspect-square w-32 overflow-hidden rounded-md bg-muted-background">
                <Image src={photo.imageUrl} alt="" fill className="object-cover" sizes="128px" />
              </div>
            </PreviewTile>
          )}
        </div>
      )}
    </Card>
  );
}
