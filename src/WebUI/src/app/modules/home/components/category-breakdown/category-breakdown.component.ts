import { ChangeDetectionStrategy, Component, input, output, signal } from '@angular/core';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { SharedModule } from '../../../../shared/shared.module';
import { BreakdownGroup, BreakdownItem, ReorderEvent } from './category-breakdown.model';

@Component({
  selector: 'app-category-breakdown',
  templateUrl: './category-breakdown.component.html',
  styleUrl: './category-breakdown.component.scss',
  standalone: true,
  imports: [SharedModule, DragDropModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CategoryBreakdownComponent {
  groups = input.required<BreakdownGroup[]>();
  currency = input.required<string>();
  entity = input.required<'expense' | 'income'>();
  tone = input<'danger' | 'success'>('danger');
  reorderable = input(false);
  readonly = input(false);
  percentFormat = input('1.0-2');

  reordered = output<ReorderEvent>();
  edit = output<BreakdownItem>();
  removed = output<number>();

  protected readonly expanded = signal<ReadonlySet<string | number>>(new Set());

  protected toggle(key: string | number): void {
    const next = new Set(this.expanded());

    if (!next.delete(key)) {
      next.add(key);
    }

    this.expanded.set(next);
  }

  protected onEdit(event: Event, item: BreakdownItem): void {
    event.stopPropagation();

    if (!this.readonly()) {
      this.edit.emit(item);
    }
  }

  protected onRemove(event: Event, id: number): void {
    event.stopPropagation();
    this.removed.emit(id);
  }
}
