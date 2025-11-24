import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable, of, shareReplay, tap } from 'rxjs';
import { SessionStorageService } from '../../../shared/services/session-storage.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private baseApiUrl = environment.apiUrl + '/api/auth';
  private user$: Observable<User | null>;
  
  private readonly cacheKey = 'user-cache';
  private readonly cacheTTL = 15 * 60 * 1000;

  constructor(
    private readonly http: HttpClient,
    private readonly sessionService: SessionStorageService) { }
  
  login(): void {
    window.location.href = `${this.baseApiUrl}/login`;
  }

  logout(): void {
    this.user$ = of(null);
    this.sessionService.clear(this.cacheKey);
    console.log(this.sessionService.load<User>(this.cacheKey));

    window.location.href = `${this.baseApiUrl}/logout`;
  }

  me(): Observable<User | null> {
    const cached = this.sessionService.load<User>(this.cacheKey);
    if (cached && !this.sessionService.isExpired(cached.fetchedAt, this.cacheTTL)) {
      if (!this.user$) {
        this.user$ = of(cached.data).pipe(shareReplay(1));
      }
      return this.user$;
    }

    this.user$ = this.http
      .get<User>(`${environment.apiUrl}/api/users/me`, { withCredentials: true })
      .pipe(
        tap((data) => this.sessionService.save<User>(this.cacheKey, data)),
        shareReplay(1)
      );

    return this.user$;
  }
}

export interface User { fullname: string, email: string, emailVerified: boolean, pictureUrl: string };
