export type Theme = "light" | "dark";

const STORAGE_KEY = "tucita-theme";

export function getStoredTheme(): Theme | null {
  const value = localStorage.getItem(STORAGE_KEY);
  return value === "dark" || value === "light" ? value : null;
}

export function getSystemTheme(): Theme {
  return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
}

export function applyTheme(theme: Theme): void {
  document.documentElement.classList.toggle("dark", theme === "dark");
  localStorage.setItem(STORAGE_KEY, theme);
}
