import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, tap, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { UserSettings } from '../models/user-settings.model';

@Injectable({ providedIn: 'root' })
export class UserSettingsService {
  private readonly baseUrl = `${environment.apiUrl}/api/user-settings`;

  private readonly state$ = new BehaviorSubject<UserSettings | null>(null);
  readonly settings$ = this.state$.asObservable();

  constructor(private readonly http: HttpClient) {}

  get current(): UserSettings | null { return this.state$.value; }

  load(): Observable<UserSettings | null> {
    return this.http.get<UserSettings | null>(this.baseUrl, { withCredentials: true })
      .pipe(tap(settings => this.state$.next(settings)));
  }

  upsert(settings: UserSettings): Observable<void> {
    const previous = this.state$.value;
    this.state$.next({ ...(previous ?? {} as UserSettings), ...settings });
    return this.http.put<void>(this.baseUrl, settings, { withCredentials: true }).pipe(
      tap(() => this.load().subscribe()),
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }
}
