import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, tap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
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
  private readonly baseUrl = `${environment.apiUrl}/api/transfers`;

  private readonly state$ = new BehaviorSubject<TransferResponse[]>([]);
  readonly transfers$ = this.state$.asObservable();

  constructor(
    private readonly http: HttpClient,
    private readonly capitalService: CapitalService
  ) {}

  get current(): TransferResponse[] { return this.state$.value; }

  load(): Observable<TransferResponse[]> {
    return this.http.get<TransferResponse[]>(this.baseUrl, { withCredentials: true })
      .pipe(tap(items => this.state$.next(items)));
  }

  refresh(): void { this.load().subscribe(); }

  getAll(): Observable<TransferResponse[]> { return this.load(); }

  create(request: CreateTransferRequest): Observable<number> {
    return this.http.post<number>(this.baseUrl, request, { withCredentials: true }).pipe(
      tap(() => {
        this.refresh();
        this.capitalService.refresh();
      })
    );
  }

  delete(id: number): Observable<void> {
    const previous = this.state$.value;
    this.state$.next(previous.filter(t => t.id !== id));
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { withCredentials: true }).pipe(
      tap(() => this.capitalService.refresh()),
      catchError(err => {
        this.state$.next(previous);
        return throwError(() => err);
      })
    );
  }
}
