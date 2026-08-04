export const PHONE_DIGIT_COUNT = 10;

/** Deja solo dígitos y recorta a la longitud de un teléfono válido. */
export function sanitizePhoneInput(value: string): string {
  return value.replace(/\D/g, "").slice(0, PHONE_DIGIT_COUNT);
}
