import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/environment.development';
import { AddCapitalRequest } from '../models/add-capital-request';
import { UpdateCapitalRequest } from '../models/update-capital-request';
import { CapitalResponse } from '../models/capital-response';
import { UpdateCapitalOrderRequest } from '../models/update-capital-order-request';
import { CurrencyType } from '../../../core/types/currency-type';
import { stringToCurrencyEnum } from '../../../shared/components/currency/functions/string-to-currency-enum';

@Injectable({
  providedIn: 'root'
})
export class CapitalService {
  private baseApiUrl = environment.apiUrl + '/api/capitals';

  constructor(private readonly httpClient: HttpClient) { }

  getMainCurrency(): { str: string, val: CurrencyType } {
    return {
      str: 'UAH',
      val: stringToCurrencyEnum('UAH') ?? CurrencyType.None
    }
  }

  getAll(searchTerm?: string, sortBy?: string, sortDirection?: string): Observable<CapitalResponse[]> {
    let params = new HttpParams();
    if (searchTerm) {
      params = params.set("searchTerm", searchTerm);
    }
    if (sortBy) {
      params = params.set("sortBy", sortBy);
    }
    if (sortDirection) {
      params = params.set("sortDirection", sortDirection);
    }

    return this.httpClient.get<CapitalResponse[]>(this.baseApiUrl, { params });
  }

  getById(id: number): Observable<CapitalResponse> {
    return this.httpClient.get<CapitalResponse>(`${this.baseApiUrl}/${id}`);
  }

  create(request: AddCapitalRequest): Observable<number> {
    return this.httpClient.post<number>(this.baseApiUrl, request);
  }

  update(id: number, request: UpdateCapitalRequest): Observable<void> {
    return this.httpClient.put<void>(`${this.baseApiUrl}/${id}`, request);
  }

  updateOrder(request: UpdateCapitalOrderRequest): Observable<void> {
    return this.httpClient.put<void>(`${this.baseApiUrl}/orders`, request);
  }

  delete(id: number): Observable<void> {
    return this.httpClient.delete<void>(`${this.baseApiUrl}/${id}`);
  }
}
