"use client";

import { useEffect, useState } from "react";
import { Button } from "@/components/ui/Button";

interface ShareLinkCardProps {
  slug: string;
}

export function ShareLinkCard({ slug }: ShareLinkCardProps) {
  const [origin, setOrigin] = useState("");
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    setOrigin(window.location.origin);
  }, []);

  const url = `${origin}/${slug}`;

  async function handleCopy() {
    try {
      await navigator.clipboard.writeText(url);
      setCopied(true);
      setTimeout(() => setCopied(false), 1500);
    } catch {
      // Clipboard API no disponible; el enlace sigue visible para copiar a mano.
    }
  }

  return (
    <div className="flex flex-col gap-3 rounded-lg border border-accent/25 bg-accent/8 p-6 shadow-sm sm:flex-row sm:items-center sm:justify-between">
      <div className="min-w-0">
        <div className="flex items-center gap-2">
          <span className="h-2 w-2 shrink-0 rounded-full bg-accent" aria-hidden />
          <p className="text-sm font-medium text-foreground">Tu enlace de reservas</p>
        </div>
        <p className="mt-1 truncate pl-4 text-sm text-muted">{url}</p>
      </div>
      <Button type="button" variant="outline" onClick={handleCopy} className="shrink-0 bg-surface">
        {copied ? "¡Copiado!" : "Copiar enlace"}
      </Button>
    </div>
  );
}
