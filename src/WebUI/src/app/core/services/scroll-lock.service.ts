import { DOCUMENT } from '@angular/common';
import { Injectable, inject } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ScrollLockService {
  private readonly root = inject(DOCUMENT).documentElement;
  private locks = 0;

  lock(): void {
    if (this.locks++ === 0) {
      this.root.classList.add('scroll-locked');
    }
  }

  unlock(): void {
    if (this.locks > 0 && --this.locks === 0) {
      this.root.classList.remove('scroll-locked');
    }
  }
}
