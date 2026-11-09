import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, tap, throwError } from 'rxjs';
import { CapitalService } from '../../modules/capital/services/capital.service';

export interface CreateTransferRequest {
  sourceCapitalId: number;
  destinationCapitalId: number;
  amount: number;
  destinationAmount: number;
}

export interface TransferResponse {
  id: number;
  amount: number;
  destinationAmount: number;
  sourceCapitalId: number;
  sourceCapitalName: string | null;
  sourceCurrency: string | null;
  destinationCapitalId: number;
  destinationCapitalName: string | null;
  destinationCurrency: string | null;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class TransferService {
  private readonly http = inject(HttpClient);
  private readonly capitalService = inject(CapitalService);

  private readonly baseUrl = '/api/transfers';

  private readonly state$ = new BehaviorSubject<TransferResponse[]>([]);
  readonly transfers$ = this.state$.asObservable();

  get current(): TransferResponse[] { return this.state$.value; }

  load(): Observable<TransferResponse[]> {
    return this.http.get<TransferResponse[]>(this.baseUrl)
      .pipe(tap(items => this.state$.next(items)));
  }

  refresh(): void { this.load().subscribe(); }

  create(request: CreateTransferRequest): Observable<number> {
    return this.http.post<number>(this.baseUrl, request).pipe(
      tap(() => {
        this.refresh();
        this.capitalService.refresh();
      })
    );
  }

  delete(id: number): Observable<void> {
    const previous = this.state$.value;
    this.state$.next(previous.filter(t => t.id !== id));
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      tap(() => this.capitalService.refresh()),
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }
}
