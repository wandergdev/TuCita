import { HTMLAttributes } from "react";
import { cn } from "@/lib/cn";

type Tone = "error" | "success";

const toneClasses: Record<Tone, string> = {
  error: "border-rose-200 bg-rose-50 text-rose-700 dark:border-rose-500/30 dark:bg-rose-500/15 dark:text-rose-300",
  success:
    "border-emerald-200 bg-emerald-50 text-emerald-700 dark:border-emerald-500/30 dark:bg-emerald-500/15 dark:text-emerald-300",
};

interface AlertProps extends HTMLAttributes<HTMLDivElement> {
  tone?: Tone;
}

export function Alert({ tone = "error", className, ...props }: AlertProps) {
  return (
    <div
      role="alert"
      className={cn(
        "rounded-md border px-4 py-3 text-sm",
        toneClasses[tone],
        className,
      )}
      {...props}
    />
  );
}
