"use client";

import { useEffect, useRef, useState } from "react";
import { cn } from "@/lib/cn";

interface TabItem<T extends string> {
  value: T;
  label: string;
}

interface TabsProps<T extends string> {
  items: TabItem<T>[];
  value: T;
  onChange: (value: T) => void;
}

export function Tabs<T extends string>({ items, value, onChange }: TabsProps<T>) {
  const scrollRef = useRef<HTMLDivElement>(null);
  const [canScrollLeft, setCanScrollLeft] = useState(false);
  const [canScrollRight, setCanScrollRight] = useState(false);

  useEffect(() => {
    const el = scrollRef.current;
    if (!el) return;

    const updateFades = () => {
      setCanScrollLeft(el.scrollLeft > 0);
      setCanScrollRight(el.scrollLeft + el.clientWidth < el.scrollWidth - 1);
    };

    updateFades();
    el.addEventListener("scroll", updateFades);
    const resizeObserver = new ResizeObserver(updateFades);
    resizeObserver.observe(el);

    return () => {
      el.removeEventListener("scroll", updateFades);
      resizeObserver.disconnect();
    };
  }, [items]);

  return (
    <div className="relative border-b border-border">
      <div ref={scrollRef} className="flex gap-1 overflow-x-auto" role="tablist">
        {items.map((item) => (
          <button
            key={item.value}
            type="button"
            role="tab"
            aria-selected={value === item.value}
            onClick={() => onChange(item.value)}
            className={cn(
              "shrink-0 border-b-2 px-3 py-2 text-sm font-medium transition-colors",
              value === item.value
                ? "border-accent text-accent"
                : "border-transparent text-muted hover:text-foreground",
            )}
          >
            {item.label}
          </button>
        ))}
      </div>
      {canScrollLeft && (
        <div className="pointer-events-none absolute inset-y-0 left-0 w-6 bg-gradient-to-r from-surface to-transparent" />
      )}
      {canScrollRight && (
        <div className="pointer-events-none absolute inset-y-0 right-0 w-6 bg-gradient-to-l from-surface to-transparent" />
      )}
    </div>
  );
}
