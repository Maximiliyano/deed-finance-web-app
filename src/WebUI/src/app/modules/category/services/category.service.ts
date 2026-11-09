import { Injectable, inject } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import {BehaviorSubject, Observable, tap} from "rxjs";
import { CategoryType } from "../../../core/types/category-type";
import { CategoryResponse } from "../models/category-model";
import { CreateCategoryRequest } from "../models/create-category-request";

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private readonly http = inject(HttpClient);

  private readonly baseApiUrl = '/api/categories';

  private readonly state$ = new BehaviorSubject<CategoryResponse[]>([]);
  readonly categories$ = this.state$.asObservable();

  load(type: CategoryType | null = null, includeDeleted: boolean | null = null): Observable<CategoryResponse[]> {
    let params = new HttpParams();

    if (type) {
      params = params.set('type', type);
    }

    if (includeDeleted) {
      params = params.set('includeDeleted', includeDeleted);
    }

    return this.http.get<CategoryResponse[]>(this.baseApiUrl, { params })
      .pipe(tap(items => {
        if (!type && !includeDeleted) {
          this.state$.next(items);
        }
      }));
  }

  create(request: CreateCategoryRequest): Observable<number> {
    return this.http.post<number>(`${this.baseApiUrl}`, request);
  }

  updateRange(categories: CategoryResponse[]): Observable<void> {
    return this.http.put<void>(`${this.baseApiUrl}/updateRange`, categories);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseApiUrl}/${id}`);
  }

  restore(id: number): Observable<CategoryResponse> {
    return this.http.post<CategoryResponse>(`${this.baseApiUrl}/${id}/restore`, {});
  }
}
