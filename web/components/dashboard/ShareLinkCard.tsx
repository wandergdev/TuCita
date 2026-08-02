"use client";

import { useEffect, useState } from "react";
import { cn } from "@/lib/cn";

interface ShareLinkCardProps {
  slug: string;
}

function LinkIcon() {
  return (
    <svg
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={2}
      strokeLinecap="round"
      strokeLinejoin="round"
      className="h-5 w-5"
    >
      <path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71" />
      <path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71" />
    </svg>
  );
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
      setTimeout(() => setCopied(false), 1600);
    } catch {
      // Clipboard API no disponible; no hay mucho más que ofrecer sin el enlace visible.
    }
  }

  return (
    <div className="fixed bottom-6 right-6 z-50 flex flex-col items-end gap-2">
      <span
        role="status"
        className={cn(
          "rounded-md bg-foreground px-3 py-1.5 text-xs font-medium text-background shadow-sm transition-opacity",
          copied ? "opacity-100" : "pointer-events-none opacity-0",
        )}
      >
        ¡Enlace copiado!
      </span>
      <button
        type="button"
        onClick={handleCopy}
        aria-label="Copiar enlace de reservas"
        title="Copiar enlace de reservas"
        className="flex h-12 w-12 items-center justify-center rounded-full bg-accent text-accent-foreground shadow-lg transition-transform hover:scale-105 hover:bg-accent-hover active:scale-95"
      >
        <LinkIcon />
      </button>
    </div>
  );
}
