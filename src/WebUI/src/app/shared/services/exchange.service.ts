import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, of, shareReplay, tap } from 'rxjs';
import { Exchange } from '../../core/models/exchange-model';
import { SessionStorageService } from './session-storage.service';

@Injectable({
  providedIn: 'root'
})
export class ExchangeService {
  private readonly http = inject(HttpClient);
  private readonly sessionService = inject(SessionStorageService);

  private readonly baseApiUrl = '/api/exchanges/';
  private readonly cacheKey = 'exchanges-cache';
  private readonly cacheTTL = 6 * 60 * 60 * 1000;

  private exchanges$: Observable<Exchange[]> | null = null;
  private fetchedAt = 0;

  getLatest(): Observable<Exchange[]> {
    if (this.exchanges$ && !this.sessionService.isExpired(this.fetchedAt, this.cacheTTL)) {
      return this.exchanges$;
    }

    const cached = this.sessionService.load<Exchange[]>(this.cacheKey);
    const hasFreshCache = !!cached && !this.sessionService.isExpired(cached.fetchedAt, this.cacheTTL);

    this.fetchedAt = hasFreshCache ? cached.fetchedAt : Date.now();
    this.exchanges$ = (hasFreshCache
      ? of(cached.data)
      : this.http.get<Exchange[]>(this.baseApiUrl).pipe(
        tap(data => this.sessionService.save<Exchange[]>(this.cacheKey, data))
      )
    ).pipe(shareReplay({ bufferSize: 1, refCount: false }));

    return this.exchanges$;
  }
}
