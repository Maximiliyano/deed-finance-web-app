import { Injectable } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Injectable({
  providedIn: 'root'
})
export class DialogService {

  constructor() { }

  hasError(formGroup: FormGroup, controlName: string, error: string): boolean {
    return (formGroup.get(controlName)?.hasError(error)) ?? false;
  }
}
