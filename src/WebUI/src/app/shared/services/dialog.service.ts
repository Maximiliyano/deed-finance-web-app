import { Injectable, Type } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { BehaviorSubject } from 'rxjs';

export interface DialogData<T = any> {
  component: Type<T>;
  data?: Partial<T>;
}

@Injectable({ providedIn: 'root' })
export class DialogService {
  private _dialogState = new BehaviorSubject<DialogData | null>(null);

  dialogState$ = this._dialogState.asObservable();

  open<T>(component: Type<T>, data?: Partial<T>): void {
    this._dialogState.next({ component, data });
  }

  close() {
    this._dialogState.next(null);
  }

  hasError(formGroup: FormGroup, controlName: string, error: string): boolean { // TODO to be removed
    return (formGroup.get(controlName)?.hasError(error)) ?? false;
  }
}
