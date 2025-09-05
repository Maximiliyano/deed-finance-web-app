import { AbstractControl, ValidationErrors } from "@angular/forms";

export function noFutureDate(control: AbstractControl): ValidationErrors | null {
  if (!control.value) return null;

  const today = new Date();
  const inputDate = new Date(control.value);

  return inputDate > today ? { futureDate: true } : null;
}
