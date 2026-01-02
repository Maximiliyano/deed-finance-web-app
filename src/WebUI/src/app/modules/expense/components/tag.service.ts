import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Tag } from '../models/tag';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TagService {
  private baseUrl = environment.apiUrl + '/api/tags';

  constructor(private http: HttpClient) {}

  search(term?: string): Observable<Tag[]> {
    let params = new HttpParams();
    if (!!term) {
      params = params.set('term', term);
    }

    return this.http.get<Tag[]>(`${this.baseUrl}`, { params, withCredentials: true });
  }

  create(expenseId: number, name: string): Observable<number> {
    return this.http.post<number>(this.baseUrl, { expenseId, name }, { withCredentials: true });
  }

  delete(tagId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${tagId}`, { withCredentials: true });
  }
}
