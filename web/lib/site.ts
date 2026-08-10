const rawSiteUrl = process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000";

export const SITE_URL = rawSiteUrl.replace(/\/$/, "");

/**
 * Dominio sin protocolo, para mostrarle al usuario su URL pública (ej.
 * "tudominio.com/jose-barber"). Cambia solo con NEXT_PUBLIC_SITE_URL —
 * nunca hardcodear un dominio en el resto del código.
 */
export const SITE_DOMAIN = SITE_URL.replace(/^https?:\/\//, "");
