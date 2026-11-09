export function toInputDateString(value: string | Date | null | undefined): string | null {
  if (!value) return null;
  const d = value instanceof Date ? value : new Date(value);

  const year = d.getFullYear();
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');

  return `${year}-${month}-${day}`;
}

export function todayDateString(): string {
  return toInputDateString(new Date())!;
}

export function inputDateToISO(input: string | null | undefined): string | null {
  if (!input) return null;
  const [y, m, d] = input.split('-').map(Number);
  const dt = new Date(y, m - 1, d);
  return dt.toISOString();
}
