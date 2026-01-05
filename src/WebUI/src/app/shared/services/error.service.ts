import { Injectable } from '@angular/core';
import { AppError } from '../../core/models/error-model';
import { Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ErrorService {
  private readonly error$ = new Subject<AppError>();

  errors$ = this.error$.asObservable();
  
  private lastError: AppError | null = null;
  private previousUrl: string | null = null;
  
  emit(error: AppError): void {
    this.error$.next(error);
  }

  setError(error: AppError, previousUrl?: string) {
    this.lastError = error;
    this.previousUrl = previousUrl ?? null;
  }

  getError(): AppError | null {
    return this.lastError;
  }

  getPreviousUrl(): string | null {
    return this.previousUrl;
  }
  
  hasError(): boolean {
    return !!this.lastError;
  }

  clear(): void {
    this.lastError = null;
    this.previousUrl = null;
  }
}