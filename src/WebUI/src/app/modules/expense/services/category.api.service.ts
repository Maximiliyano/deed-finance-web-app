import { HttpClient } from "@angular/common/http";
import { environment } from "../../../../environments/environment.development";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class CategoryApiService {
  private baseUrl = environment.apiUrl + "/api/categories";

  constructor(private readonly http: HttpClient) { }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
