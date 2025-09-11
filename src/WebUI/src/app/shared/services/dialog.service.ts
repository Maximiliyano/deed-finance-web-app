import { Injectable, Type } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface DialogData<T = any, R = any> {
  component: Type<T>;
  data?: Partial<T>;
  onSubmit?: (result: R) => void;
}

@Injectable({ providedIn: 'root' })
export class DialogService {
  private _dialogStack = new BehaviorSubject<DialogData[]>([]);
  dialogStack$ = this._dialogStack.asObservable();

  open<T>(dialogData: DialogData<T>): void {
    const stack = [...this._dialogStack.value, dialogData];
    this._dialogStack.next(stack);
  }

  close(): void {
    const stack = [...this._dialogStack.value];
    stack.pop();
    this._dialogStack.next(stack);
  }

  closeAll(): void {
    this._dialogStack.next([]);
  }
}
