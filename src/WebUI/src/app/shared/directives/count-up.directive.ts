import { DestroyRef, Directive, ElementRef, NgZone, effect, inject, input } from '@angular/core';
import { CurrencySymbolPipe } from '../components/currency/pipes/currency-symbol-pipe';

const DURATION_MS = 800;

@Directive({
  selector: '[appCountUp]',
  standalone: true
})
export class CountUpDirective {
  readonly appCountUp = input.required<number | null | undefined>();
  readonly countUpCurrency = input.required<string>();

  private readonly element = inject<ElementRef<HTMLElement>>(ElementRef).nativeElement;
  private readonly zone = inject(NgZone);
  private readonly format = new CurrencySymbolPipe();
  private readonly reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

  private shown = 0;
  private frame = 0;

  constructor() {
    inject(DestroyRef).onDestroy(() => cancelAnimationFrame(this.frame));
    effect(() => this.run(this.appCountUp() ?? 0, this.countUpCurrency()));
  }

  private run(target: number, currency: string): void {
    cancelAnimationFrame(this.frame);
    const from = this.shown;

    if (this.reduceMotion || from === target) {
      this.render(target, currency);
      return;
    }

    const start = performance.now();
    const step = (now: number): void => {
      const progress = Math.min((now - start) / DURATION_MS, 1);
      const eased = progress === 1 ? 1 : 1 - Math.pow(2, -10 * progress);
      this.render(from + (target - from) * eased, currency);
      if (progress < 1) this.frame = requestAnimationFrame(step);
    };

    this.zone.runOutsideAngular(() => (this.frame = requestAnimationFrame(step)));
  }

  private render(value: number, currency: string): void {
    this.shown = value;
    this.element.textContent = this.format.transform(value, currency);
  }
}
