import { Directive, ElementRef, HostListener, forwardRef, inject, input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Directive({
  selector: 'input[appNumeric]',
  standalone: true,
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => NumericInputDirective), multi: true }],
  host: {
    type: 'text',
    autocomplete: 'off',
    autocapitalize: 'off',
    spellcheck: 'false',
    '[attr.inputmode]': 'integer() ? "numeric" : "decimal"'
  }
})
export class NumericInputDirective implements ControlValueAccessor {
  readonly appNumeric = input<'integer' | 'decimal' | ''>('');

  private readonly element = inject<ElementRef<HTMLInputElement>>(ElementRef).nativeElement;
  private onChange: (value: number | null) => void = () => {};
  private onTouched: () => void = () => {};

  integer(): boolean {
    return this.appNumeric() === 'integer';
  }

  @HostListener('input')
  protected onInput(): void {
    const cleaned = this.sanitize(this.element.value);

    if (cleaned !== this.element.value) {
      const caret = (this.element.selectionStart ?? cleaned.length) - (this.element.value.length - cleaned.length);
      this.element.value = cleaned;
      this.element.setSelectionRange(Math.max(caret, 0), Math.max(caret, 0));
    }

    const parsed = cleaned === '' || cleaned === '.' ? null : Number(cleaned);
    this.onChange(parsed);
  }

  @HostListener('blur')
  protected onBlur(): void {
    if (this.element.value.endsWith('.')) {
      this.element.value = this.element.value.slice(0, -1);
    }

    this.onTouched();
  }

  writeValue(value: number | string | null | undefined): void {
    this.element.value = value === null || value === undefined ? '' : String(value);
  }

  registerOnChange(fn: (value: number | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(disabled: boolean): void {
    this.element.disabled = disabled;
  }

  private sanitize(raw: string): string {
    const normalized = raw.replace(/,/g, '.');

    if (this.integer()) {
      return normalized.replace(/\D/g, '').replace(/^0+(?=\d)/, '');
    }

    const stripped = normalized.replace(/[^\d.]/g, '');
    const dot = stripped.indexOf('.');
    const head = dot === -1 ? stripped : stripped.slice(0, dot);
    const tail = dot === -1 ? '' : `.${stripped.slice(dot + 1).replace(/\./g, '')}`;

    return head.replace(/^0+(?=\d)/, '') + tail;
  }
}
