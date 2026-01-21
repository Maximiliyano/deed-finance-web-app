import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { Observable } from 'rxjs';
import { IncomeResponses } from '../models/income-response';
import { CreateIncomeRequest } from '../models/create-income-request';

@Injectable({
  providedIn: 'root'
})
export class IncomeService {
  private baseUrl = environment.apiUrl + "/api/incomes";

  constructor(private readonly http: HttpClient) { }

  getAll(): Observable<IncomeResponses> {
    return this.http.get<IncomeResponses>(this.baseUrl, { withCredentials: true });
  }

  create(request: CreateIncomeRequest): Observable<number> {
    return this.http.post<number>(this.baseUrl, request, { withCredentials: true });
  }
}
