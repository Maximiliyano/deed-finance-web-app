import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, catchError, Observable, of, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  userSubject = new BehaviorSubject<User | null>(null);
  user$ = this.userSubject.asObservable();

  private loaded = false;

  constructor(private readonly http: HttpClient) { }
  
  login(): void {
    window.location.href = `${environment.apiUrl}/api/auth/login`;
  }

  logout(): void {
    window.location.href = `${environment.apiUrl}/api/auth/logout`;
  }

  me(): Observable<User | null> {
    if (this.loaded) return this.user$;

    this.http.get<User>(`${environment.apiUrl}/api/users/me`, { withCredentials: true })
      .pipe(
        catchError(() => of(null)),
        tap(user => {
          this.userSubject.next(user);
          this.loaded = true;
        })
      )
      .subscribe();

      return this.user$;
  }
}

export interface User { sid: string, fullname: string, email: string, emailVerified: boolean, pictureUrl: string };
