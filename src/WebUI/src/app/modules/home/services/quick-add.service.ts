import { Injectable, inject } from '@angular/core';
import { catchError, firstValueFrom, of } from 'rxjs';
import { CategoryType } from '../../../core/types/category-type';
import { DialogService } from '../../../shared/components/dialogs/services/dialog.service';
import { PopupMessageService } from '../../../shared/services/popup-message.service';
import { TransferService } from '../../../shared/services/transfer.service';
import { CapitalResponse } from '../../capital/models/capital-response';
import { CapitalService } from '../../capital/services/capital.service';
import { CategoryResponse } from '../../category/models/category-model';
import { CategoriesDialogComponent } from '../../category/category-details-dialog/categories-dialog-component';
import { CategoryService } from '../../category/services/category.service';
import { ExpenseService } from '../../expense/services/expense.service';
import { IncomeService } from '../../incomes/services/income.service';
import { QuickTransactionDialogComponent } from '../components/quick-transaction-dialog/quick-transaction-dialog.component';
import { TransferDialogComponent, TransferDialogData } from '../components/transfer-dialog/transfer-dialog.component';
import { DashboardStore } from './dashboard-store.service';

export type QuickKind = 'expense' | 'income';

interface QuickDefaults {
  capitalId?: number;
  expenseCategoryId?: number;
  incomeCategoryId?: number;
}

const DEFAULTS_KEY = 'deed-quick-defaults';
const NO_FILTER = { searchTerm: null, sortBy: null, sortDirection: null, filterBy: null };

@Injectable({ providedIn: 'root' })
export class QuickAddService {
  private readonly store = inject(DashboardStore);
  private readonly capitalService = inject(CapitalService);
  private readonly categoryService = inject(CategoryService);
  private readonly expenseService = inject(ExpenseService);
  private readonly incomeService = inject(IncomeService);
  private readonly transferService = inject(TransferService);
  private readonly dialogService = inject(DialogService);
  private readonly popup = inject(PopupMessageService);

  async addTransaction(kind: QuickKind): Promise<void> {
    const [capitals, categories] = await Promise.all([this.loadCapitals(), this.loadCategories()]);
    const type = kind === 'expense' ? CategoryType.Expenses : CategoryType.Incomes;

    const ref = this.dialogService.open(QuickTransactionDialogComponent, {
      data: {
        type: kind,
        capitals: kind === 'expense' ? capitals.filter(capital => !capital.onlyForSavings) : capitals,
        categories: categories.filter(category => category.type === type),
        defaults: this.readDefaults()
      }
    });

    ref.afterClosed$.subscribe(result => {
      if (!result) return;

      const request$ = kind === 'expense' ? this.expenseService.create(result) : this.incomeService.create(result);
      request$.subscribe({
        next: () => {
          this.popup.success(kind === 'expense' ? 'Expense added' : 'Income added');
          this.capitalService.refresh();
          this.saveDefaults(kind, result.capitalId, result.categoryId);
        },
        error: () => this.popup.error(kind === 'expense' ? 'Failed to add expense' : 'Failed to add income')
      });
    });
  }

  async transfer(sourceCapitalId?: number): Promise<void> {
    const capitals = await this.loadCapitals();

    const data: TransferDialogData = {
      capitals,
      exchanges: this.store.exchanges(),
      sourceCapitalId: sourceCapitalId ?? null
    };
    const ref = this.dialogService.open(TransferDialogComponent, { data });

    ref.afterClosed$.subscribe(result => {
      if (!result) return;

      this.transferService.create(result).subscribe({
        next: () => this.popup.success('Transfer completed'),
        error: () => this.popup.error('Failed to create transfer')
      });
    });
  }

  async manageCategories(kind: QuickKind): Promise<boolean> {
    const type = kind === 'expense' ? CategoryType.Expenses : CategoryType.Incomes;
    const all = await firstValueFrom(
      this.categoryService.load(type, true).pipe(catchError(() => of([] as CategoryResponse[])))
    );

    const ref = this.dialogService.open(CategoriesDialogComponent, {
      data: {
        type: kind === 'expense' ? 'Expense' : 'Income',
        currency: this.store.userCurrency(),
        originalCategories: all.filter(category => !category.isDeleted),
        deletedCategories: all.filter(category => category.isDeleted)
      }
    });

    const responses: CategoryResponse[] | null = await firstValueFrom(ref.afterClosed$, { defaultValue: null });
    if (!responses?.length) return true;

    const normalized = responses.map(category => ({ ...category, periodType: Number(category.periodType) }));

    try {
      await firstValueFrom(this.categoryService.updateRange(normalized));
      this.popup.success('Categories updated');
    } catch {
      this.popup.error('Failed to update categories');
      return false;
    }

    await firstValueFrom(this.categoryService.load().pipe(catchError(() => of([]))));
    return true;
  }

  private async loadCapitals(): Promise<CapitalResponse[]> {
    if (this.store.capitals().length === 0) {
      await firstValueFrom(this.capitalService.load(NO_FILTER).pipe(catchError(() => of([]))));
    }

    return this.store.capitals();
  }

  private async loadCategories(): Promise<CategoryResponse[]> {
    const current = await firstValueFrom(this.categoryService.categories$);
    if (current.length > 0) return current;

    return firstValueFrom(this.categoryService.load().pipe(catchError(() => of([] as CategoryResponse[]))));
  }

  private readDefaults(): QuickDefaults {
    try {
      return JSON.parse(localStorage.getItem(DEFAULTS_KEY) ?? '{}') as QuickDefaults;
    } catch {
      return {};
    }
  }

  private saveDefaults(kind: QuickKind, capitalId: number, categoryId: number): void {
    const next: QuickDefaults = {
      ...this.readDefaults(),
      capitalId,
      [kind === 'expense' ? 'expenseCategoryId' : 'incomeCategoryId']: categoryId
    };

    try {
      localStorage.setItem(DEFAULTS_KEY, JSON.stringify(next));
    } catch {
      return;
    }
  }
}
