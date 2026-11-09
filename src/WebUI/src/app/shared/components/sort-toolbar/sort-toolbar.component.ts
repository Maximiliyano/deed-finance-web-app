import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { NgClass } from '@angular/common';

export interface SortOption<TKey extends string = string> {
  key: TKey;
  title: string;
  icon: string;
}

export type AmountSort = 'custom' | 'default' | 'amount-desc' | 'amount-asc' | 'name';

export const NAMED_AMOUNT_SORT_OPTIONS: SortOption[] = [
  { key: 'custom', title: 'Custom order', icon: 'fa-grip-vertical' },
  { key: 'amount-desc', title: 'Highest first', icon: 'fa-arrow-down-wide-short' },
  { key: 'amount-asc', title: 'Lowest first', icon: 'fa-arrow-up-short-wide' },
  { key: 'name', title: 'A to Z', icon: 'fa-arrow-down-a-z' }
];

export const AMOUNT_SORT_OPTIONS: SortOption<AmountSort>[] = [
  { key: 'custom', title: 'Custom order', icon: 'fa-grip-vertical' },
  { key: 'default', title: 'Default order', icon: 'fa-percent' },
  { key: 'amount-desc', title: 'Highest first', icon: 'fa-arrow-down-wide-short' },
  { key: 'amount-asc', title: 'Lowest first', icon: 'fa-arrow-up-short-wide' },
  { key: 'name', title: 'A to Z', icon: 'fa-arrow-down-a-z' }
];

@Component({
  selector: 'app-sort-toolbar',
  templateUrl: './sort-toolbar.component.html',
  styleUrl: './sort-toolbar.component.scss',
  standalone: true,
  imports: [NgClass],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SortToolbarComponent {
  options = input.required<SortOption[]>();
  value = input.required<string>();
  changed = output<string>();
}
