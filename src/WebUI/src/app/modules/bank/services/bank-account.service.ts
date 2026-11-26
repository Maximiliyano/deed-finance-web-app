import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, switchMap, tap } from 'rxjs';
import { BankAccount } from '../models/bank.model';

@Injectable({ providedIn: 'root' })
export class BankAccountService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/bank-accounts';

  private readonly state$ = new BehaviorSubject<BankAccount[]>([]);
  readonly accounts$ = this.state$.asObservable();

  load(): Observable<BankAccount[]> {
    return this.http.get<BankAccount[]>(this.baseUrl).pipe(tap(accounts => this.state$.next(accounts)));
  }

  refresh(): Observable<BankAccount[]> {
    return this.http.post<number>(`${this.baseUrl}/refresh`, null).pipe(switchMap(() => this.load()));
  }

  connect(id: number): Observable<BankAccount[]> {
    return this.http.post<void>(`${this.baseUrl}/${id}/connect`, null).pipe(switchMap(() => this.load()));
  }
}
