import { Pipe, PipeTransform } from "@angular/core";

export function enumToOptions<T extends object>(
  enumObj: T,
  options?: { exclude?: string[] }
): { key: string; value: T[keyof T] }[] {
  const exclude = options?.exclude ?? [];

  return Object.keys(enumObj)
    .filter(k => isNaN(Number(k)))
    .filter(k => !exclude.includes(k))
    .map(k => ({
      key: k,
      value: enumObj[k as keyof T]
    }));
}

@Pipe({
  name: 'enumText',
  standalone: false
})
export class EnumTextPipe implements PipeTransform {
  transform<T extends string | number>(
    value: T,
    enumObj: Record<string, string | number>
  ): string {
    if (value === null || value === undefined) return '';

    const numericValue = Number(value);

    if (!isNaN(numericValue) && enumObj[numericValue] !== undefined) {
      return String(enumObj[numericValue]);
    }

    if (typeof value === 'string' && enumObj[value] !== undefined) {
      return value;
    }

    return (enumObj as any)[value];
  }
}
