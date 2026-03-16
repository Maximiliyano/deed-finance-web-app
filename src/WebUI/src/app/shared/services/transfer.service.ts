import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

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

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<TransferResponse[]> {
    return this.http.get<TransferResponse[]>(this.baseUrl, { withCredentials: true });
  }

  create(request: CreateTransferRequest): Observable<number> {
    return this.http.post<number>(this.baseUrl, request, { withCredentials: true });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { withCredentials: true });
  }
}
