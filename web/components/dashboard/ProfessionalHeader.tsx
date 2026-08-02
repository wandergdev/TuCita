"use client";

import { useEffect, useState } from "react";
import { Badge } from "@/components/ui/Badge";
import { getProfessionalProfile } from "@/lib/api";
import { CATEGORY_LABELS } from "@/lib/labels";
import type { ProfessionalPublicProfile } from "@/types/api";

interface ProfessionalHeaderProps {
  slug: string;
}

export function ProfessionalHeader({ slug }: ProfessionalHeaderProps) {
  const [profile, setProfile] = useState<ProfessionalPublicProfile | null>(null);

  useEffect(() => {
    let cancelled = false;

    getProfessionalProfile(slug)
      .then((result) => {
        if (!cancelled) setProfile(result);
      })
      .catch(() => {
        // El resto del panel sigue funcionando aunque el perfil público falle.
      });

    return () => {
      cancelled = true;
    };
  }, [slug]);

  if (!profile) {
    return (
      <div className="flex items-center gap-4 rounded-lg border border-border bg-surface p-6 shadow-sm">
        <div className="h-12 w-12 shrink-0 animate-pulse rounded-full bg-muted-background" />
        <div className="flex-1">
          <div className="h-4 w-40 animate-pulse rounded bg-muted-background" />
          <div className="mt-2 h-3 w-24 animate-pulse rounded bg-muted-background" />
        </div>
      </div>
    );
  }

  const initial = profile.fullName.trim().charAt(0).toUpperCase() || "?";

  return (
    <div className="flex items-center gap-4 rounded-lg border border-border bg-surface p-6 shadow-sm">
      <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-accent/15 text-lg font-semibold text-accent">
        {initial}
      </div>
      <div className="min-w-0">
        <p className="truncate text-lg font-semibold text-foreground">{profile.fullName}</p>
        <div className="mt-1 flex flex-wrap items-center gap-2">
          <Badge className="bg-accent/10 text-accent">{CATEGORY_LABELS[profile.category]}</Badge>
          <span className="truncate text-sm text-muted">tucita.com/{profile.slug}</span>
        </div>
      </div>
    </div>
  );
}
