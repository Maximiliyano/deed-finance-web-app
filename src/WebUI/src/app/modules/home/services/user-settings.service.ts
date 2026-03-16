import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { UserSettings } from '../models/user-settings.model';

@Injectable({ providedIn: 'root' })
export class UserSettingsService {
  private readonly baseUrl = `${environment.apiUrl}/api/user-settings`;

  constructor(private readonly http: HttpClient) {}

  get(): Observable<UserSettings | null> {
    return this.http.get<UserSettings | null>(this.baseUrl, { withCredentials: true });
  }

  upsert(settings: UserSettings): Observable<void> {
    return this.http.put<void>(this.baseUrl, settings, { withCredentials: true });
  }
}
