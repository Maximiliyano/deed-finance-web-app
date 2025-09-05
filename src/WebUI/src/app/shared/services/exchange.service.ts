import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, shareReplay, tap } from 'rxjs';
import { Exchange } from '../../core/models/exchange-model';
import { environment } from '../../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class ExchangeService {
  private baseApiUrl = environment.apiUrl + '/api/exchanges/';


  constructor(private readonly http: HttpClient) { }

  getAll(): Observable<Exchange[]> {
    const cacheKey = 'exchanges';
    const cacheTimeKey = 'exchanges_time';
    const cacheDuration = 12 * 60 * 60 * 1000;

    const cachedData = localStorage.getItem(cacheKey);
    const cacheTime = localStorage.getItem(cacheTimeKey);

    if (cachedData && cacheTime) {
      const isExpired = (Date.now() - +cacheTime) > cacheDuration;

      if (isExpired) {
        localStorage.removeItem(cacheKey);
        localStorage.removeItem(cacheTimeKey);
      } else {
        return of(JSON.parse(cachedData));
      }
    }

  return this.http.get<Exchange[]>(this.baseApiUrl).pipe(
    tap(exchanges => {
      localStorage.setItem(cacheKey, JSON.stringify(exchanges));
      localStorage.setItem(cacheTimeKey, Date.now().toString());
    }),
    shareReplay({ bufferSize: 1, refCount: true })
  );
  }
}
