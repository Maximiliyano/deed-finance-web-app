import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment.development';
import { AddCapitalRequest } from '../models/add-capital-request';
import { UpdateCapitalRequest } from '../models/update-capital-request';
import { CapitalResponse } from '../models/capital-response';

@Injectable({
  providedIn: 'root'
})
export class CapitalService {
  private baseApiUrl = environment.apiUrl + '/api/capitals';

  constructor(private readonly httpClient: HttpClient) { }

  getAll(): Observable<CapitalResponse[]> {
    return this.httpClient.get<CapitalResponse[]>(this.baseApiUrl);
  }

  getById(id: number): Observable<CapitalResponse> {
    return this.httpClient.get<CapitalResponse>(`${this.baseApiUrl}/${id}`);
  }

  create(request: AddCapitalRequest): Observable<number> {
    return this.httpClient.post<number>(this.baseApiUrl, request);
  }

  update(id: number, request: UpdateCapitalRequest): Observable<Object> {
    return this.httpClient.put(`${this.baseApiUrl}/${id}`, request);
  }

  delete(id: number): Observable<Object> {
    return this.httpClient.delete(`${this.baseApiUrl}/${id}`);
  }
}
