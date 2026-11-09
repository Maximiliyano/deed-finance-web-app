import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, of, switchMap, tap, throwError } from 'rxjs';
import { CURRENCY_DEFAULT } from '../../../core/types/currency-type';
import { UserSettings } from '../models/user-settings.model';

@Injectable({ providedIn: 'root' })
export class UserSettingsService {
  private readonly http = inject(HttpClient);

  private readonly baseUrl = '/api/user-settings';

  private readonly state$ = new BehaviorSubject<UserSettings | null>(null);
  readonly settings$ = this.state$.asObservable();

  get current(): UserSettings | null { return this.state$.value; }

  load(): Observable<UserSettings | null> {
    return this.http.get<UserSettings | null>(this.baseUrl)
      .pipe(tap(settings => this.state$.next(settings)));
  }

  patch(changes: Partial<UserSettings>): Observable<void> {
    const base$ = this.current ? of(this.current) : this.load();
    return base$.pipe(
      switchMap(base => this.upsert({ ...(base ?? { currency: CURRENCY_DEFAULT }), ...changes }))
    );
  }

  upsert(settings: UserSettings): Observable<void> {
    const previous = this.state$.value;
    this.state$.next({ ...(previous ?? {} as UserSettings), ...settings });
    return this.http.put<void>(this.baseUrl, settings).pipe(
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }
}
