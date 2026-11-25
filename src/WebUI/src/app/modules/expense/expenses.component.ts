import { AuthService } from '../auth/services/auth-service';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ExpenseCategoryResponse } from './models/expense-category-response';
import { DialogService } from '../../shared/components/dialogs/services/dialog.service';
import { AddExpenseDialogComponent } from './components/add-expense-dialog/add-expense-dialog.component';
import { CapitalResponse } from '../capital/models/capital-response';
import { ExpenseService } from './services/expense.service';
import { CapitalService } from '../capital/services/capital.service';

import { CategoryService } from '../category/services/category.service';
import { CategoryType } from '../../core/types/category-type';
import { CreateExpenseRequest } from './models/create-expense-request';
import { ExpenseResponse } from './models/expense-response';
import { ConfirmDialogComponent } from '../../shared/components/dialogs/confirm-dialog/confirm-dialog.component';
import { PopupMessageService } from '../../shared/services/popup-message.service';
import { ActivatedRoute, Router } from '@angular/router';
import { SelectOptionModel } from '../../shared/components/forms/models/select-option-model';
import { PerPeriodType } from '../../core/types/per-period-type';
import { EditExpenseDialogComponent } from './components/edit-expense-dialog.component/edit-expense-dialog.component';
import { UpdateExpenseRequest } from './models/update-expense.request';
import { CategoriesDialogComponent } from '../category/category-details-dialog/categories-dialog-component';
import { CategoryResponse } from '../category/models/category-model';
import { TagService } from './components/tag.service';
import { Tag } from './models/tag';
import { UtilityBillService } from './services/utility-bill.service';
import { CreateUtilityBillRequest, PayUtilityBillRequest, UtilityBillResponse } from './models/utility-bill';
import {
  UtilityBillDialogComponent,
  UtilityBillDialogData
} from './components/utility-bill-dialog/utility-bill-dialog.component';
import {
  PayUtilityBillDialogComponent,
  PayUtilityBillDialogData
} from './components/pay-utility-bill-dialog/pay-utility-bill-dialog.component';
import { CURRENCY_DEFAULT } from '../../core/types/currency-type';

@Component({
  selector: 'app-expenses',
  templateUrl: './expenses.component.html',
  styleUrl: './expenses.component.scss',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExpensesComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly expenseService = inject(ExpenseService);
  private readonly categoryService = inject(CategoryService);
  private readonly capitalService = inject(CapitalService);
  private readonly tagService = inject(TagService);
  private readonly utilityBillService = inject(UtilityBillService);
  private readonly authService = inject(AuthService);
  private readonly dialogService = inject(DialogService);
  private readonly popupMessageService = inject(PopupMessageService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);

  expenseCategories: ExpenseCategoryResponse[] = [];
  capitals: CapitalResponse[] = [];

  currentCategories: CategoryResponse[] = [];
  deletedCategories: CategoryResponse[] = [];
  tags: Tag[] = [];

  selectedCapital: CapitalResponse | null = null;
  private capitalIdFromQuery: number | null = null;
  openedExpensesCategoryId: number | null = null;

  defaultCurrency: string;
  PerPeriodType = PerPeriodType;

  activeTab: 'expenses' | 'bills' = 'expenses';
  utilityBills: UtilityBillResponse[] = [];

  get pendingBills(): UtilityBillResponse[] {
    return this.utilityBills.filter(b => b.isActive && !b.isPaidThisMonth);
  }

  get paidBills(): UtilityBillResponse[] {
    return this.utilityBills.filter(b => b.isPaidThisMonth);
  }

  get inactiveBills(): UtilityBillResponse[] {
    return this.utilityBills.filter(b => !b.isActive);
  }

  setTab(tab: 'expenses' | 'bills'): void {
    this.activeTab = tab;
    if (tab === 'bills' && this.utilityBills.length === 0) {
      this.fetchUtilityBills();
    }
  }

  fetchUtilityBills(): void {
    this.utilityBillService
      .getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: bills => {
          this.utilityBills = bills;
          this.cdr.markForCheck();
        }
      });
  }

  openUtilityBillDialog(bill?: UtilityBillResponse): void {
    const data: UtilityBillDialogData = {
      bill,
      capitals: this.capitals,
      categories: this.currentCategories
    };
    const ref = this.dialogService.open(UtilityBillDialogComponent, { data });
    ref.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((result: CreateUtilityBillRequest | null) => {
      if (!result) return;
      if (bill) {
        this.utilityBillService
          .update(bill.id, result)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => {
              this.popupMessageService.success('Utility bill updated');
              this.fetchUtilityBills();
            }
          });
      } else {
        this.utilityBillService
          .create(result)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => {
              this.popupMessageService.success('Utility bill added');
              this.fetchUtilityBills();
            }
          });
      }
    });
  }

  payUtilityBill(bill: UtilityBillResponse): void {
    const data: PayUtilityBillDialogData = { bill };
    const ref = this.dialogService.open(PayUtilityBillDialogComponent, { data });
    ref.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((result: PayUtilityBillRequest | null) => {
      if (!result) return;
      this.utilityBillService
        .pay(bill.id, result)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.popupMessageService.success(`${bill.name} marked paid`);
            this.fetchUtilityBills();
            this.fetchExpenses();
            this.fetchCapitals();
          }
        });
    });
  }

  deleteUtilityBill(bill: UtilityBillResponse): void {
    const dialogRef = this.dialogService.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete utility bill',
        message: `Delete "${bill.name}"? This won't affect already-paid expenses.`,
        icon: 'danger'
      }
    });
    dialogRef.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (confirmed: boolean) => {
        if (!confirmed) return;
        this.utilityBillService
          .delete(bill.id)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => {
              this.popupMessageService.success('Utility bill deleted');
              this.fetchUtilityBills();
            }
          });
      }
    });
  }

  get capitalOptions(): SelectOptionModel[] {
    return this.capitals.map(x => {
      return { key: x.name, value: x.id };
    });
  }

  get categoryOptions(): SelectOptionModel[] {
    return this.currentCategories.map(x => {
      return { key: x.name, value: x.id };
    });
  }

  get capitalBalance(): number {
    return this.selectedCapital?.balance ?? this.capitals.reduce((sum, capital) => sum + capital.balance, 0);
  }

  get capitalTotalExpenses(): number {
    return (
      this.selectedCapital?.totalExpense ??
      this.expenseCategories.flatMap(ec => ec.expenses).reduce((sum, expense) => sum + expense.amount, 0)
    );
  }

  get capitalCurrency(): string {
    return this.selectedCapital?.currency ?? this.defaultCurrency;
  }

  ngOnInit(): void {
    document.title = 'Deed - Expenses';

    this.authService
      .me()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: user => {
          this.defaultCurrency = user?.currency ?? CURRENCY_DEFAULT;
          this.cdr.markForCheck();
        }
      });

    this.route.queryParamMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      this.capitalIdFromQuery = Number(params.get('capitalId')) || null;
      this.selectCapitalFromQuery();
    });

    this.fetchExpenses();
    this.fetchCapitals();
    this.fetchCategories();
    this.fetchTags();
  }

  isPlannedPeriodShown(expenseCategory: ExpenseCategoryResponse): boolean {
    return expenseCategory.plannedPeriodAmount > 0.0 && expenseCategory.periodType !== PerPeriodType.None;
  }

  isCategorySumGreaterPlannedPeriod(expenseCategory: ExpenseCategoryResponse): boolean {
    return expenseCategory.plannedPeriodAmount - expenseCategory.categorySum > 0;
  }

  fetchTags(): void {
    this.tagService
      .search()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: tags => {
          this.tags = tags;
          this.cdr.markForCheck();
        }
      });
  }

  fetchCategories(): void {
    this.categoryService
      .load(CategoryType.Expenses, true)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: responses => {
          this.currentCategories = responses.filter(c => !c.isDeleted);
          this.deletedCategories = responses.filter(c => c.isDeleted);
          this.cdr.markForCheck();
        }
      });
  }

  fetchExpenses(): void {
    this.expenseService
      .load(this.selectedCapital?.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: responses => {
          this.expenseCategories = responses;
          this.cdr.markForCheck();
        }
      });
  }

  fetchCapitals(): void {
    this.capitalService
      .load({
        filterBy: 'onlyForSavings',
        searchTerm: null,
        sortBy: null,
        sortDirection: null
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: responses => {
          this.capitals = responses;
          this.selectCapitalFromQuery();
        }
      });
  }

  private selectCapitalFromQuery(): void {
    this.selectedCapital = this.capitals.find(c => c.id === this.capitalIdFromQuery) ?? null;
    this.cdr.markForCheck();
  }

  onCapitalChange(capital: CapitalResponse | null): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { capitalId: capital?.id ?? null },
      queryParamsHandling: 'merge'
    });
    this.selectedCapital = capital;
    this.fetchExpenses();
  }

  private refreshExpenses(message: string): void {
    this.popupMessageService.success(message);
    this.fetchExpenses();
    this.fetchCapitals();
  }

  toggleExpensesList(categoryId: number): void {
    if (this.openedExpensesCategoryId === categoryId) {
      this.openedExpensesCategoryId = null;
    } else {
      this.openedExpensesCategoryId = categoryId;
    }
  }

  toggleCategories(): void {
    const categoriesDialogRef = this.dialogService.open(CategoriesDialogComponent, {
      data: {
        type: 'Expense',
        currency: this.capitalCurrency,
        originalCategories: this.currentCategories,
        deletedCategories: this.deletedCategories
      }
    });

    categoriesDialogRef.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (responses: CategoryResponse[]) => {
        if (responses?.length) {
          const normalized = responses.map(c => ({
            ...c,
            periodType: Number(c.periodType)
          }));

          this.categoryService
            .updateRange(normalized)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
              next: () => {
                this.popupMessageService.success('Categories successfully updated.');
                this.fetchCategories();
                this.fetchExpenses();
              }
            });
        } else {
          this.fetchCategories();
          this.fetchExpenses();
        }
      }
    });
  }

  toggleCreateDialog(): void {
    const dialogRef = this.dialogService.open(AddExpenseDialogComponent, {
      data: {
        capitalsOptions: this.capitalOptions,
        categoryOptions: this.categoryOptions,
        tags: this.tags
      }
    });

    dialogRef.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (request: CreateExpenseRequest) => {
        if (request) {
          this.expenseService
            .create(request)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
              next: () => this.refreshExpenses('New expense added')
            });
        }
      }
    });
  }

  toggleEditExpenseDialog(expense: ExpenseResponse, oldCategoryId: number): void {
    const dialogRef = this.dialogService.open(EditExpenseDialogComponent, {
      data: {
        expense: expense,
        categoryId: oldCategoryId,
        categoryOptions: this.categoryOptions,
        capitalOptions: this.capitalOptions,
        tags: this.tags
      }
    });

    dialogRef.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (request: UpdateExpenseRequest | null) => {
        if (request) {
          this.expenseService
            .update(request)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
              next: () => this.refreshExpenses('Expense updated.')
            });
        }
      }
    });
  }

  deleteExpense(id: number): void {
    const dialogRef = this.dialogService.open(ConfirmDialogComponent, {
      data: {
        title: 'Deletion of expense',
        message: `Are you sure you want to perform this action?`,
        icon: 'danger'
      }
    });

    dialogRef.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (result: boolean) => {
        if (result) {
          this.expenseService
            .delete(id)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
              next: () => this.refreshExpenses('Expense deleted')
            });
        }
      }
    });
  }
}
