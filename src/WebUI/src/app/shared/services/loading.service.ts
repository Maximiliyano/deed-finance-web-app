import {Injectable} from '@angular/core';
import {BehaviorSubject} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private requestCount = 0;
  private delayTimer: ReturnType<typeof setTimeout> | null = null;

  isLoading$ = this.loadingSubject.asObservable();

  show(): void {
    this.requestCount++;
    if (this.requestCount === 1 && !this.delayTimer) {
      this.delayTimer = setTimeout(() => {
        if (this.requestCount > 0) {
          this.loadingSubject.next(true);
        }
        this.delayTimer = null;
      }, 300);
    }
  }

  hide(): void {
    this.requestCount = Math.max(0, this.requestCount - 1);
    if (this.requestCount === 0) {
      if (this.delayTimer) {
        clearTimeout(this.delayTimer);
        this.delayTimer = null;
      }
      this.loadingSubject.next(false);
    }
  }
}
