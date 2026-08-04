/** Convierte texto libre en un slug válido: minúsculas, sin tildes, solo a-z/0-9/guiones. */
export function sanitizeSlugInput(value: string): string {
  return value
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "") // quita tildes/diacriticos (NFD)
    .toLowerCase()
    .replace(/\s+/g, "-") // espacios -> guion
    .replace(/[^a-z0-9-]/g, "") // quita cualquier otro caracter no permitido
    .replace(/-+/g, "-"); // colapsa guiones repetidos
}
