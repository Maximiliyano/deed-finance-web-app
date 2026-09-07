import {Injectable} from "@angular/core";
import {environment} from "../../../../environments/environment";
import { HttpClient, HttpParams } from "@angular/common/http";
import {BehaviorSubject, Observable, tap} from "rxjs";
import { CategoryType } from "../../../core/types/category-type";
import { CategoryResponse } from "../models/category-model";
import { CreateCategoryRequest } from "../models/create-category-request";

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private baseApiUrl = environment.apiUrl + '/api/categories';

  private readonly state$ = new BehaviorSubject<CategoryResponse[]>([]);
  readonly categories$ = this.state$.asObservable();

  constructor(private readonly http: HttpClient) { }

  get objects(): CategoryResponse[]
  {
    return this.state$.value;
  }

  getAll(type: CategoryType | null = null, includeDeleted: boolean | null = null): Observable<CategoryResponse[]> {
    let params = new HttpParams();

    if (type) {
      params = params.set('type', type);
    }

    if (includeDeleted) {
      params = params.set('includeDeleted', includeDeleted);
    }

    return this.http.get<CategoryResponse[]>(this.baseApiUrl, { params })
      .pipe(tap(items => this.state$.next(items)));
  }

  create(request: CreateCategoryRequest): Observable<number> {
    return this.http.post<number>(`${this.baseApiUrl}`, request, {withCredentials: true});
  }

  updateRange(categories: CategoryResponse[]): Observable<void> {
    return this.http.put<void>(`${this.baseApiUrl}/updateRange`, categories, {withCredentials: true});
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseApiUrl}/${id}`);
  }

  restore(id: number): Observable<CategoryResponse> {
    return this.http.post<CategoryResponse>(`${this.baseApiUrl}/${id}/restore`, {}, {withCredentials: true});
  }
}
