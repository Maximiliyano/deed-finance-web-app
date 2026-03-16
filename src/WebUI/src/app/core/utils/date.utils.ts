export function todayDateString(): string {
  return new Date().toISOString().split('T')[0];
}
