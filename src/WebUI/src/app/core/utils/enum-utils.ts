export function getEnumKeys<E extends object>(enumObj: E): string[] {
  return Object.keys(enumObj).filter(key => isNaN(Number(key)));
}
