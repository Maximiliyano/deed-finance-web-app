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
    return this.http.get<Exchange[]>(this.baseApiUrl);
  }
}
