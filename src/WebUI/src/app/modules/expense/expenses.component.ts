import { Component, OnDestroy, OnInit } from '@angular/core';
import { ExpenseCategoryResponse } from './models/expense-category-response';
import { DialogService } from '../../shared/components/dialogs/services/dialog.service';
import { AddExpenseDialogComponent } from './components/add-expense-dialog/add-expense-dialog.component';
import { CapitalResponse } from '../capital/models/capital-response';
import { ExpenseService } from './services/expense.service';
import { CapitalService } from '../capital/services/capital.service';
import { Subject, takeUntil, throttleTime } from 'rxjs';
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

@Component({
    selector: 'app-expenses',
    templateUrl: './expenses.component.html',
    styleUrl: './expenses.component.scss',
    standalone: false
})
export class ExpensesComponent implements OnInit, OnDestroy {
  expenseCategories: ExpenseCategoryResponse[] = [];
  capitals: CapitalResponse[] = [];

  currentCategories: CategoryResponse[] = [];
  deletedCategories: CategoryResponse[] = [];

  selectedCapital: CapitalResponse | null = null;
  openedExpensesCategoryId: number | null = null;

  defaultCurrency: string;
  PerPeriodType=  PerPeriodType;

  private $unsubscribe = new Subject<void>();

  constructor(
    private readonly router: Router,
    private readonly route: ActivatedRoute,
    private readonly expenseService: ExpenseService,
    private readonly categoryService: CategoryService,
    private readonly capitalService: CapitalService,
    private readonly dialogService: DialogService,
    private readonly popupMessageService: PopupMessageService
  ) {}

  get capitalOptions(): SelectOptionModel[] {
    return this.capitals.map(x => { return { key: x.name, value: x.id } })
  }

  get categoryOptions(): SelectOptionModel[] {
    return this.currentCategories.map(x => { return { key: x.name, value: x.id } });
  }

  get capitalBalance(): number {
    return this.selectedCapital?.balance ?? this.capitals.reduce((sum, capital) => sum + capital.balance, 0);
  }

  get capitalTotalExpenses(): number {
    return this.selectedCapital?.totalExpense ?? this.expenseCategories.flatMap(ec => ec.expenses).reduce((sum, expense) => sum + expense.amount, 0);
  }

  get capitalCurrency(): string {
    return this.selectedCapital?.currency ?? this.defaultCurrency;
  }

  ngOnInit(): void {
    document.title = "Deed - Expenses";

    this.defaultCurrency = this.capitalService.getMainCurrency().str;

    this.fetchExpenses();
    this.fetchCapitals();
    this.fetchCategories();
  }

  ngOnDestroy(): void {
    this.$unsubscribe.next();
    this.$unsubscribe.complete();
  }

  isPlannedPeriodShown(expenseCategory: ExpenseCategoryResponse): boolean {
    return expenseCategory.plannedPeriodAmount > 0.0 && expenseCategory.periodType !== PerPeriodType.None;
  }

  isCategorySumGreaterPlannedPeriod(expenseCategory: ExpenseCategoryResponse): boolean {
    return expenseCategory.plannedPeriodAmount - expenseCategory.categorySum > 0;
  }

  fetchCategories(): void {
    this.categoryService
      .getAll(CategoryType.Expenses, true)
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe({
        next: (responses) => {
          this.currentCategories = responses.filter(c => !c.isDeleted);
          this.deletedCategories = responses.filter(c => c.isDeleted);
        }
      });
  }

  fetchExpenses(): void {
    this.expenseService
      .getAllByCategories(this.selectedCapital?.id)
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe({
        next: (responses) => this.expenseCategories = responses
      });
  }

  fetchCapitals(): void {
    this.capitalService
      .getAll({
        filterBy: 'onlyForSavings',
        searchTerm: null,
        sortBy: null,
        sortDirection: null
      })
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe({
        next: (responses) => {
          this.capitals = responses;
          this.handleQueryParams();
        }
      });
  }

  handleQueryParams(): void {
    this.route.queryParamMap.subscribe({
      next: (params) => {
        const queryCapitalId = params.get('capitalId');
        const capital = this.capitals.find(c => c.id === Number(queryCapitalId));
        
        this.selectedCapital = capital ?? null;
      }
    })
  }

  onCapitalChange(capital: CapitalResponse | null): void {
    if (capital) {
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { capitalId: capital.id },
        queryParamsHandling: 'merge'
      })
    }
    this.selectedCapital = capital;
    this.fetchExpenses();
  }

  toggleExpensesList(categoryId: number): void {
    if (this.openedExpensesCategoryId === categoryId) {
      this.openedExpensesCategoryId = null
    } else {
      this.openedExpensesCategoryId = categoryId;
    }
  }

  toggleCategories(): void {
    // TODO when expense added, add it in categories
    const categoriesDialogRef = this.dialogService.open(CategoriesDialogComponent, {
      data: {
        type: "Expense",
        currency: this.capitalCurrency,
        originalCategories: this.currentCategories,
        deletedCategories: this.deletedCategories
      },
    });
    
    categoriesDialogRef
      .afterClosed$
      .subscribe({
        next: (responses: CategoryResponse[]) => {
          if (!responses) return;
          
          if (responses.length === 0) return;

          const normalized  = responses.map(c => ({
            ...c,
            periodType: Number(c.periodType)
          }));

          this.categoryService.updateRange(normalized)
            .pipe(takeUntil(this.$unsubscribe))
            .subscribe({
              next: () => {
                this.popupMessageService.success('Categories successfully updated.');
              }
            });
          }
        });
  }

  toggleCreateDialog(): void {
    const dialogRef = this.dialogService.open(AddExpenseDialogComponent, {
        data: {
          capitalsOptions: this.capitalOptions,
          categoryOptions: this.categoryOptions,
        },
    });

    dialogRef
      .afterClosed$
      .subscribe({
        next: (request: CreateExpenseRequest) => {
          if (request) {
            this.expenseService.create(request)
              .pipe(takeUntil(this.$unsubscribe))
              .subscribe({
                next: (id) => {
                  this.addExpenseToList(id, request)
                }
              });
          }
        }
      }
    );
  }

  addExpenseToList(expenseId: number, request: CreateExpenseRequest): void {
    const existingCategoryExpense = this.expenseCategories.find(c => c.categoryId === request.categoryId);

    const newExpense: ExpenseResponse = {
      id: expenseId,
      capitalId: request.capitalId,
      amount: request.amount,
      paymentDate: request.paymentDate,
      purpose: request.purpose
    };

    if (existingCategoryExpense) {
      existingCategoryExpense.categorySum = existingCategoryExpense.categorySum + request.amount;
      existingCategoryExpense.expenses.push(newExpense);
    } else {
      const category = this.currentCategories.find(c => c.id === request.categoryId);
      if (!category) return;

      this.expenseCategories.push({
        categoryId: request.categoryId,
        name: category.name,
        categorySum: request.amount,
        percentage: 0,
        plannedPeriodAmount: category.periodAmount,
        periodType: category.periodType,
        expenses: [newExpense]
      });
    }

    const capital = this.capitals.find(c => c.id === request.capitalId);
    if (!capital) return;

    capital.balance -= newExpense.amount;

    const totalSum = this.expenseCategories.reduce((sum, c) => sum + c.categorySum, 0);
    this.expenseCategories.forEach(c => {
        c.percentage = totalSum === 0
            ? 0
            : parseFloat(((c.categorySum / totalSum) * 100).toFixed(2));
    });

    this.popupMessageService.success(`New expense added`);
  }

  toggleEditExpenseDialog(expense: ExpenseResponse, oldCategoryId: number): void {
    const dialogRef = this.dialogService.open(EditExpenseDialogComponent, {
      data: {
        expense: expense,
        categoryId: oldCategoryId,
        categoryOptions: this.categoryOptions,
        capitalOptions: this.capitalOptions
      }
    });

    dialogRef
      .afterClosed$
      .subscribe({
        next: (request: UpdateExpenseRequest | null) => {
          if (request) {
            this.expenseService
              .update(request)
              .pipe(takeUntil(this.$unsubscribe))
              .subscribe({
                next: () => this.updateExpense(request, oldCategoryId)
              })
          }
        }
      });
  }

  deleteExpense(id: number, categoryId: number): void {
    const dialogRef = this.dialogService.open(ConfirmDialogComponent, {
      data: {
        title: 'Deletion of expense',
        message: `Are you sure you want to perform this action?`,
        icon: 'danger'
      }
    });

    dialogRef
      .afterClosed$
      .subscribe({
        next: (result: boolean) => {
          if (result) {
            this.expenseService
              .delete(id)
              .pipe(takeUntil(this.$unsubscribe))
              .subscribe({
                next: () => {
                  this.removeExpenseFromList(id, categoryId);
                }
              });
          }
        }
      }
    );
  }

  updateExpense(update: UpdateExpenseRequest, currentCategoryId: number): void {
    // TODO handle update in old category
    const currentExpenseCategory = this.expenseCategories.find(ec => ec.categoryId === currentCategoryId);
    if (!currentExpenseCategory) return;

    const currentExpense = currentExpenseCategory.expenses.find(e => e.id === update.id);
    if (!currentExpense) return;

    const currentCapital = this.capitals.find(c => c.id === currentExpense.capitalId);
    if (!currentCapital) return;
    
    currentExpense.purpose = update.purpose ?? null;
    currentExpense.paymentDate = new Date(update.date ?? currentExpense.paymentDate);

    // TODO handle update in old capital
    if (!!update.amount) {
      const difference = update.amount - currentExpense.amount;
      
      currentExpense.amount = update.amount;

      currentExpenseCategory.categorySum += difference;
      currentCapital.balance -= difference;
    }

    // TODO handle moving from old category to new, removing from old
    if (!!update.capitalId) {
      const newCapital = this.capitals.find(c => c.id === update.capitalId);
      if (!newCapital) return;

      currentCapital.balance += currentExpense.amount;

      newCapital.balance -= currentExpense.amount;

      currentExpense.capitalId = update.capitalId;
    }

    // TODO handle update from old capital to new, removing from current
    if (!!update.categoryId && update.categoryId !== currentCategoryId) {
      const newCategory = this.expenseCategories.find(ec => ec.categoryId === update.categoryId);
      
      currentExpenseCategory.categorySum -= currentExpense.amount;
      currentExpenseCategory.expenses = currentExpenseCategory.expenses.filter(e => e.id !== update.id);

      if (currentExpenseCategory.categorySum === 0) {
        this.expenseCategories = this.expenseCategories.filter(ec => ec.categoryId !== currentCategoryId);
      }

      if (newCategory) {
        newCategory.expenses.push(currentExpense);
        newCategory.categorySum += currentExpense.amount;
      }
      else {
        const category = this.currentCategories.find(c => c.id == update.categoryId);
        if (!category) return;

        this.expenseCategories.push({
          categoryId: category.id,
          name: category.name,
          categorySum: currentExpense.amount,
          percentage: 0, // TODO
          plannedPeriodAmount: category.periodAmount,
          periodType: category.periodType,
          expenses: [currentExpense]
        });

        const totalSum = this.expenseCategories.reduce((sum, c) => sum + c.categorySum, 0);
        this.expenseCategories.forEach(c => {
            c.percentage = totalSum === 0
                ? 0
                : parseFloat(((c.categorySum / totalSum) * 100).toFixed(2));
        });
      }
    }

    this.popupMessageService.success('Expense updated.');
  }

  removeExpenseFromList(id: number, categoryId: number): void {
    const response = this.expenseCategories.find(e => e.categoryId === categoryId);

    if (!response) {
      return;
    }

    const expense = response.expenses.find(e => e.id === id);

    if (!expense) {
      return
    }

    const capital = this.capitals.find(c => c.id === expense.capitalId);
    if (!capital) return;

    capital.balance += expense.amount;

    response.categorySum -= expense.amount;
    response.expenses = response.expenses.filter(e => e.id !== id);

    if (response.categorySum === 0) {
      this.expenseCategories = this.expenseCategories.filter(e => e.categoryId !== response.categoryId);
    }

    this.popupMessageService.success('Expense deleted');
  }
}
