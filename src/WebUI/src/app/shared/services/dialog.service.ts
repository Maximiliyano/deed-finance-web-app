import { Injectable, Type } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { BehaviorSubject } from 'rxjs';

export interface DialogData<T = any, R = any> {
  component: Type<T>;
  data?: Partial<T>;
  onSubmit?: (result: R) => void;
}

@Injectable({ providedIn: 'root' })
export class DialogService {
  private _dialogState = new BehaviorSubject<DialogData | null>(null);
  dialogState$ = this._dialogState.asObservable();

  open<T>(dialogData: DialogData<T>): void {
    this._dialogState.next(dialogData);
  }

  close(): void {
    this._dialogState.next(null);
  }

  hasError(formGroup: FormGroup, controlName: string, error: string): boolean { // TODO to be removed
    return (formGroup.get(controlName)?.hasError(error)) ?? false;
  }
}
