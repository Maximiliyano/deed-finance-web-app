/** Convert ISO-ish date/string/Date to 'YYYY-MM-DD' in local timezone */

export function toInputDateString(value: string | Date | null | undefined): string | null {
  if (!value) return null;
  const d = value instanceof Date ? value : new Date(value);

  // Adjust to local date components (avoid timezone shift)
  const year = d.getFullYear();
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');

  return `${year}-${month}-${day}`;
}

export function inputDateToISO(input: string | null | undefined): string | null {
  if (!input) return null;
  // create Date from local midnight, then toISOString (UTC)
  const [y, m, d] = input.split('-').map(Number);
  const dt = new Date(y, m - 1, d);
  return dt.toISOString(); // backend-friendly ISO
}
