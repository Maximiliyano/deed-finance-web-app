import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, shareReplay, tap } from 'rxjs';
import { Exchange } from '../../core/models/exchange-model';
import { environment } from '../../../environments/environment';
import { SessionStorageService } from './session-storage.service';

@Injectable({
  providedIn: 'root'
})
export class ExchangeService {
  private baseApiUrl = environment.apiUrl + '/api/exchanges/';
  private exchanges$: Observable<Exchange[]>;
  
  private readonly cacheKey = 'exchanges-cache';
  private readonly cacheTTL = 6 * 60 * 60 * 1000;

  constructor(
    private readonly http: HttpClient,
    private readonly sessionService: SessionStorageService) { }

  getLatest(): Observable<Exchange[]> {
    const cached = this.sessionService.load<Exchange[]>(this.cacheKey);
    if (cached && !this.sessionService.isExpired(cached.fetchedAt, this.cacheTTL)) {
      if (!this.exchanges$) {
        this.exchanges$ = of(cached.data).pipe(shareReplay(1));
      }
      return this.exchanges$;
    }

    this.exchanges$ = this.http
      .get<Exchange[]>(this.baseApiUrl, { withCredentials: true })
      .pipe(
        tap((data) => this.sessionService.save<Exchange[]>(this.cacheKey, data)),
        shareReplay(1)
      );

    return this.exchanges$;
  }
}
