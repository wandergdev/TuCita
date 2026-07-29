"use client";

import { useEffect, useState } from "react";
import { Card } from "@/components/ui/Card";
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
    <Card className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
      <div className="min-w-0">
        <p className="text-sm font-medium text-foreground">Tu enlace de reservas</p>
        <p className="truncate text-sm text-muted">{url}</p>
      </div>
      <Button type="button" variant="outline" onClick={handleCopy} className="shrink-0">
        {copied ? "¡Copiado!" : "Copiar enlace"}
      </Button>
    </Card>
  );
}
