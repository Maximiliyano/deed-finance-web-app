import { Injectable, Type } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { BehaviorSubject } from 'rxjs';

export interface DialogData<T = any> {
  component: Type<T>;
  data?: Partial<T>;
  onSubmit?: (result: any) => void;
}

@Injectable({ providedIn: 'root' })
export class DialogService {
  private _dialogState = new BehaviorSubject<DialogData | null>(null);
  dialogState$ = this._dialogState.asObservable();

  private _resolver: ((value: any) => void) | null = null;

  open<T>(component: Type<T>, data?: Partial<T> & { onSubmit?: (result: any) => void }): Promise<any> {
    const dialogData: DialogData<T> = { component, ...data };
    this._dialogState.next(dialogData);

    return new Promise<any>((resolve) => {
      this._resolver = resolve;
    });
  }

  close(result?: any): void {
    this._dialogState.next(null);
    if (this._resolver) {
      this._resolver(result);
      this._resolver = null;
    }
  }

  hasError(formGroup: FormGroup, controlName: string, error: string): boolean { // TODO to be removed
    return (formGroup.get(controlName)?.hasError(error)) ?? false;
  }
}
