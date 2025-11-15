import { Subject } from 'rxjs';

export class DialogRef<R = any> {
  private readonly _afterClosed = new Subject<R | undefined>();
  readonly afterClosed$ = this._afterClosed.asObservable();

  close(result?: R) {
    this._afterClosed.next(result);
    this._afterClosed.complete();
  }
}
