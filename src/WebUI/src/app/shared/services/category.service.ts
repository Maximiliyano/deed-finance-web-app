import {Injectable} from "@angular/core";
import {environment} from "../../../environments/environment";
import { HttpClient, HttpParams } from "@angular/common/http";
import {Observable} from "rxjs";
import {CategoryResponse} from "../../core/models/category-model";
import { CategoryType } from "../../core/types/category-type";
import { CreateCategoryRequest } from "../components/category/models/create-category-request";

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private baseApiUrl = environment.apiUrl + '/api/categories';

  constructor(private readonly http: HttpClient) { }

  getAll(type: CategoryType | null = null): Observable<CategoryResponse[]> {
    let params = new HttpParams();

    if (type != null) {
      params = params.set('type', type);
    }

    return this.http.get<CategoryResponse[]>(this.baseApiUrl, { params, withCredentials: true });
  }

  create(request: CreateCategoryRequest): Observable<number> {
    return this.http.post<number>(`${this.baseApiUrl}`, request, { withCredentials: true });
  }

  updateRange(categories: CategoryResponse[]): Observable<void> {
    return this.http.put<void>(`${this.baseApiUrl}/updateRange`, categories, { withCredentials: true });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseApiUrl}/${id}`, { withCredentials: true });
  }
}
