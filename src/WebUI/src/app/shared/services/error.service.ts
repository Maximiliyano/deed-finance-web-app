import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ErrorService {
  private lastError: { status?: number; message?: string } | null = null;
  private previousUrl: string | null = null;

  setError(error: { status?: number; message?: string }, previousUrl?: string) {
    this.lastError = error;
    this.previousUrl = previousUrl ?? null;
  }

  getError() {
    return this.lastError;
  }

  getPreviousUrl() {
    return this.previousUrl;
  }

  clear() {
    this.lastError = null;
    this.previousUrl = null;
  }

  hasError() {
    return !!this.lastError;
  }
}