import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {BehaviorSubject, catchError, Observable, of, shareReplay, tap} from 'rxjs';
import {User} from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly userSubject = new BehaviorSubject<User | null>(null);
  readonly user$ = this.userSubject.asObservable();

  private loaded = false;
  private pending$: Observable<User | null> | null = null;

  constructor(private readonly http: HttpClient) { }

  login(): void {
    const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
    window.location.href = `${window.location.host}/api/auth/login?returnUrl=${returnUrl}`;
  }

  logout(): void {
    window.location.href = `${window.location.host}/api/auth/logout`;
  }

  invalidate(): void {
    this.loaded = false;
    this.pending$ = null;
    this.userSubject.next(null);
  }

  me(): Observable<User | null> {
    if (this.loaded) return of(this.userSubject.value);

    if (!this.pending$) {
      this.pending$ = this.http.get<User>(`/api/users/me`, {withCredentials: true})
        .pipe(
          catchError(() => of(null)),
          tap(user => {
            this.userSubject.next(user);
            this.loaded = true;
            this.pending$ = null;
          }),
          shareReplay(1)
        );
    }

    return this.pending$;
  }
}
