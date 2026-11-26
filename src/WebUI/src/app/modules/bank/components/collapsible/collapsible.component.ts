import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, input, signal } from '@angular/core';

let nextId = 0;

@Component({
  selector: 'app-collapsible',
  templateUrl: './collapsible.component.html',
  styleUrl: './collapsible.component.scss',
  standalone: true,
  imports: [NgClass],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CollapsibleComponent implements OnInit {
  readonly heading = input.required<string>();
  readonly icon = input<string | null>(null);
  readonly storageKey = input<string | null>(null);

  protected readonly open = signal(true);
  protected readonly uid = `collapsible-${nextId++}`;

  ngOnInit(): void {
    const key = this.storageKey();

    if (!key) return;

    try {
      this.open.set(localStorage.getItem(key) !== 'closed');
    } catch {
      this.open.set(true);
    }
  }

  protected toggle(): void {
    this.open.update(value => !value);

    const key = this.storageKey();

    if (!key) return;

    try {
      localStorage.setItem(key, this.open() ? 'open' : 'closed');
    } catch {
      return;
    }
  }
}
