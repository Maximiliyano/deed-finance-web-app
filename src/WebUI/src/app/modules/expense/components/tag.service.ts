import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Tag } from '../models/tag';

@Injectable({
  providedIn: 'root'
})
export class TagService {
  private readonly http = inject(HttpClient);

  private baseUrl = '/api/tags';

  search(term?: string): Observable<Tag[]> {
    let params = new HttpParams();
    if (!!term) {
      params = params.set('term', term);
    }

    return this.http.get<Tag[]>(`${this.baseUrl}`, { params });
  }

  create(expenseId: number, name: string): Observable<number> {
    return this.http.post<number>(this.baseUrl, { expenseId, name });
  }

  delete(tagId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${tagId}`);
  }
}
