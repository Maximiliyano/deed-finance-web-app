import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { IncomeService } from './services/income.service';

import { DialogService } from '../../shared/components/dialogs/services/dialog.service';
import { CreateIncomeDialogComponent } from './components/create-income-dialog.component/create-income-dialog.component';
import { SelectOptionModel } from '../../shared/components/forms/models/select-option-model';
import { PopupMessageService } from '../../shared/services/popup-message.service';
import { CreateIncomeRequest } from './models/create-income-request';
import { CategoryResponse } from '../category/models/category-model';
import { CapitalService } from '../capital/services/capital.service';
import { CategoryService } from '../category/services/category.service';
import { CategoryType } from '../../core/types/category-type';

@Component({
  selector: 'app-incomes.component',
  imports: [CommonModule],
  templateUrl: './incomes.component.html',
  styleUrl: './incomes.component.scss',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class IncomesComponent implements OnInit {
  private readonly incomeService = inject(IncomeService);
  private readonly capitalService = inject(CapitalService);
  private readonly categoryService = inject(CategoryService);
  private readonly dialogService = inject(DialogService);
  private readonly popup = inject(PopupMessageService);
  private readonly destroyRef = inject(DestroyRef);

  incomes = toSignal(this.incomeService.incomes$, { initialValue: [] });
  capitals = toSignal(this.capitalService.capitals$, { initialValue: [] });
  categories = signal<CategoryResponse[]>([]);

  get capitalOptions(): SelectOptionModel[] {
    return this.capitals().map(x => ({ key: x.name, value: x.id }));
  }

  get categoryOptions(): SelectOptionModel[] {
    return this.categories().map(x => ({ key: x.name, value: x.id }));
  }

  onDelete(id: number) {
    this.incomeService.delete(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => this.popup.success('Income removed')
    });
  }

  capitalName(id: number | null): string {
    if (!id) return '—';
    return this.capitals().find(c => c.id === id)?.name ?? '—';
  }

  categoryName(id: number | null): string {
    if (!id) return '—';
    return this.categories().find(c => c.id === id)?.name ?? '—';
  }

  ngOnInit(): void {
    document.title = 'Deed - Incomes';

    this.incomeService.load().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
    this.capitalService.load({ searchTerm: null, sortBy: null, sortDirection: null, filterBy: null })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe();
    this.categoryService.load(CategoryType.Incomes)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(categories => this.categories.set(categories));
  }

  openCreateIncomeDialog(): void {
    const dialogRef = this.dialogService.open(CreateIncomeDialogComponent, {
      data: {
        capitalsOptions: this.capitalOptions,
        categoryOptions: this.categoryOptions
      }
    });

    dialogRef
      .afterClosed$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((request: CreateIncomeRequest) => {
        if (!request) return;
        this.incomeService
          .create(request)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => this.popup.success('Income created successfully.')
          });
      });
  }
}
