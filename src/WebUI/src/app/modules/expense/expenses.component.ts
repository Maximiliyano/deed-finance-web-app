import { Component, OnDestroy, OnInit } from '@angular/core';
import { ExpenseCategoryResponse } from './models/expense-category-response';
import { DialogService } from '../../shared/services/dialog.service';
import { ExpenseDialogComponent } from './components/expense-dialog/expense-dialog.component';
import { CapitalResponse } from '../capital/models/capital-response';
import { ExpenseService } from './services/expense.service';
import { CapitalService } from '../capital/services/capital.service';
import { Subject, takeUntil } from 'rxjs';
import { CategoryResponse } from '../../core/models/category-model';
import { CategoryService } from '../../shared/services/category.service';
import { CategoryType } from '../../core/types/category-type';
import { CreateExpenseRequest } from './models/create-expense-request';
import { ExpenseResponse } from './models/expense-response';
import { CategoriesDialogComponent } from './components/categories-dialog-component/categories-dialog-component';
import { ConfirmDialogComponent } from '../../shared/components/dialogs/confirm-dialog/confirm-dialog.component';
import { PopupMessageService } from '../../shared/services/popup-message.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-expenses',
  templateUrl: './expenses.component.html',
  styleUrl: './expenses.component.scss'
})
export class ExpensesComponent implements OnInit, OnDestroy {
  expenseCategories: ExpenseCategoryResponse[] = [];
  capitals: CapitalResponse[] = [];
  categories: CategoryResponse[] = [];

  selectedCapital: CapitalResponse | null = null;
  openedExpensesCategoryId: number | null = null;

  defaultCurrency: string;

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
    return expenseCategory.plannedPeriodAmount === 0.0 || expenseCategory.periodType === 'None';
  }

  fetchCategories(): void {
    this.categoryService
      .getAll(CategoryType.Expenses)
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe({
        next: (responses) => this.categories = responses
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
      .getAll(undefined, undefined, undefined, 'onlyForSavings')
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
        if (capital) {
          this.selectedCapital = capital;
        } else {
          this.selectedCapital = null;
        }
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
    this.dialogService.open({
      component: CategoriesDialogComponent,
      data: {
        categories: this.categories
      }
    });
  }

  toggleCreateDialog(): void {
    this.dialogService.open({
      component: ExpenseDialogComponent,
      data: {
        capitalsOptions: this.capitals.map(x => { return { key: x.name, value: x.id } }),
        categoryOptions: this.categories.map(x => { return { key: x.name, value: x.id } })
      },
      onSubmit: (request: CreateExpenseRequest) => {
        if (request) {
          this.expenseService.create(request)
            .pipe(takeUntil(this.$unsubscribe))
            .subscribe({
              next: (id) => {
                this.addExpenseToList(id, request)
                this.dialogService.close();
              }
            });
        } else {
          this.dialogService.close();
        }
      }
    });
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
      const category = this.categories.find(c => c.id === request.categoryId);
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

    this.popupMessageService.success(`Expense added`);
  }

  deleteExpense(id: number, categoryId: number): void {
    this.dialogService.open({
      component: ConfirmDialogComponent,
      data: {
        title: 'capital',
        action: 'remove'
      },
      onSubmit: (result) => {
        if (result) {
          this.expenseService
            .delete(id)
            .pipe(takeUntil(this.$unsubscribe))
            .subscribe({
              next: () => {
                this.removeExpenseFromList(id, categoryId);
                this.dialogService.close();
              }
            });
        } else {
          this.dialogService.close();
        }
      }
    })
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
