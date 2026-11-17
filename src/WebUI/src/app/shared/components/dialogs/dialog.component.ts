import { ChangeDetectionStrategy, Component, ElementRef, ViewContainerRef, Type, Injector, AfterViewInit, OnDestroy, NgZone, ViewChild, Input, inject } from '@angular/core';
import { DialogRef } from './models/dialog-ref';

const DISMISS_DISTANCE = 120;
const DISMISS_VELOCITY = 0.6;
const SHEET_QUERY = '(max-width: 575px)';
const NO_DRAG_SELECTOR = 'input, textarea, select, [cdkDrag], [cdkDragHandle]';

@Component({
    selector: 'app-dialog',
    templateUrl: './dialog.component.html',
    styleUrl: './dialog.component.scss',
    standalone: true,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class DialogComponent implements AfterViewInit, OnDestroy {
  private readonly zone = inject(NgZone);

  @ViewChild('dialogContainer', { read: ViewContainerRef })
  container!: ViewContainerRef;

  @ViewChild('modal', { static: true })
  modal!: ElementRef<HTMLElement>;

  @Input() index = 0;
  @Input() childComponentType!: Type<any>;
  @Input() childInjector!: Injector;

  private startY = 0;
  private startTime = 0;
  private offset = 0;
  private tracking = false;
  private dragging = false;
  private closing = false;

  ngAfterViewInit(): void {
    this.container.createComponent(this.childComponentType, {
      injector: this.childInjector
    });

    this.zone.runOutsideAngular(() => {
      const el = this.modal.nativeElement;
      el.addEventListener('touchstart', this.onTouchStart, { passive: true });
      el.addEventListener('touchmove', this.onTouchMove, { passive: false });
      el.addEventListener('touchend', this.onTouchEnd, { passive: true });
      el.addEventListener('touchcancel', this.onTouchEnd, { passive: true });
    });
  }

  ngOnDestroy(): void {
    const el = this.modal.nativeElement;
    el.removeEventListener('touchstart', this.onTouchStart);
    el.removeEventListener('touchmove', this.onTouchMove);
    el.removeEventListener('touchend', this.onTouchEnd);
    el.removeEventListener('touchcancel', this.onTouchEnd);
  }

  close(): void {
    const ref = this.childInjector.get(DialogRef, null);
    ref?.close();
  }

  private readonly onTouchStart = (event: TouchEvent): void => {
    const target = event.target as HTMLElement;

    this.tracking = !this.closing
      && window.matchMedia(SHEET_QUERY).matches
      && this.modal.nativeElement.scrollTop <= 0
      && !target.closest(NO_DRAG_SELECTOR);

    this.dragging = false;
    this.offset = 0;
    this.startY = event.touches[0].clientY;
    this.startTime = event.timeStamp;
  };

  private readonly onTouchMove = (event: TouchEvent): void => {
    if (!this.tracking) return;

    const delta = event.touches[0].clientY - this.startY;

    if (!this.dragging) {
      if (delta < 0 || this.modal.nativeElement.scrollTop > 0) {
        this.tracking = false;
        return;
      }

      if (delta < 6) return;
      this.dragging = true;
      this.modal.nativeElement.style.transition = 'none';
    }

    event.preventDefault();
    this.offset = Math.max(delta, 0);
    this.modal.nativeElement.style.transform = `translateY(${this.offset}px)`;
  };

  private readonly onTouchEnd = (event: TouchEvent): void => {
    if (!this.dragging) {
      this.tracking = false;
      return;
    }

    this.tracking = false;
    this.dragging = false;

    const el = this.modal.nativeElement;
    const velocity = this.offset / Math.max(event.timeStamp - this.startTime, 1);

    if (this.offset > DISMISS_DISTANCE || velocity > DISMISS_VELOCITY) {
      this.closing = true;
      el.style.transition = 'transform 0.2s ease-in';
      el.style.transform = 'translateY(100%)';
      setTimeout(() => this.zone.run(() => this.close()), 180);
      return;
    }

    el.style.transition = 'transform 0.2s ease-out';
    el.style.transform = '';
  };
}
