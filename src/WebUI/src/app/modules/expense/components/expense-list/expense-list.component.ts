import {Component, OnDestroy, OnInit} from '@angular/core';
import {BehaviorSubject, Subject, takeUntil} from 'rxjs';
import {CapitalResponse} from '../../../capital/models/capital-response';
import {CapitalService} from "../../../capital/services/capital.service";
import {Periods} from "../../models/periods";
import {CategoryResponse} from "../../../../core/models/category-model";
import {ExpenseDialogComponent} from "../expense-dialog/expense-dialog.component";
import { ActivatedRoute } from '@angular/router';
import { ExpenseService } from '../../services/expense.service';
import { ExpenseResponse } from '../../models/expense-response';
import { CategoryService } from '../../../../shared/services/category.service';
import { CategoryType } from '../../../../core/types/category-type';
import { DialogService } from '../../../../shared/services/dialog.service';
import { AddExpenseRequest } from '../../models/add-expense-request';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';

@Component({
  selector: 'app-expense-list',
  templateUrl: './expense-list.component.html',
  styleUrl: './expense-list.component.scss'
})
export class ExpenseListComponent implements OnInit, OnDestroy {
  categories$ = new BehaviorSubject<CategoryResponse[]>([]);

  capitals: CapitalResponse[] = [];
  categories: CategoryResponse[] = [];
  dialogCategories: CategoryResponse[] = [];
  selectedCapital: CapitalResponse | null = null;

  expenses: ExpenseResponse[] = [];
  totalExpenses: number = 0;

  totalCapitalsBalance: number = 0;
  totalCapitalsExpense: number = 0;

  periods: string[] = Object.values(Periods);
  defaultCurrency: string = 'UAH';
  defaultPeriod: string = Periods.Month;

  selectedPeriod: string = this.defaultPeriod;
  startDate: Date | null = null;
  endDate: Date | null = null;
  allTime: boolean = false;

  private $unsubscribe = new Subject<void>();

  constructor(
    private readonly route: ActivatedRoute,
    private readonly popupService: PopupMessageService,
    private readonly expenseService: ExpenseService,
    private readonly capitalService: CapitalService,
    private readonly categoryService: CategoryService,
    private readonly dialogService: DialogService) {}

  ngOnInit(): void {
    this.fetchCapitals();

    this.fetchCategories();

    this.fetchExpenses();

    this.updateDateRange();
  }

  ngOnDestroy(): void {
    this.$unsubscribe.next();
    this.$unsubscribe.complete();
  }

  updateDateRange() {
    const today = new Date();

    switch (this.selectedPeriod) {
      case Periods.Day:
        this.startDate = this.endDate = today;
        break;

      case Periods.Week:
        this.startDate = new Date(today.setDate(today.getDate() - 7));
        this.endDate = new Date();
        break;

      case Periods.Month:
        this.startDate = new Date(today.setMonth(today.getMonth() - 1));
        this.endDate = new Date();
        break;

      case Periods.Year:
        this.startDate = new Date(today.setFullYear(today.getFullYear() - 1));
        this.endDate = new Date();
        break;

      case Periods.Custom:
        break;

      default:
        this.startDate = this.endDate = null;
    }
  }

  selectTab(period: string): void {
    this.selectedPeriod = period;

    if (period == Periods.Custom) {
      // TODO custom
    } else {
      this.updateDateRange();
    }
  }

  openDialogAddExpense(): void {
    let category: CategoryResponse | null = null;

    this.dialogService.open({
      component: ExpenseDialogComponent,
      data: {
        capitals: this.capitals,
        categories: this.dialogCategories
      },
      onSubmit: (request: AddExpenseRequest) => {
        if (request) {
          this.expenseService.add(request)
            .pipe(takeUntil(this.$unsubscribe))
            .subscribe({
              next: (id) => {
                this.popupService.success('The expense is successfully added.');

                const response: ExpenseResponse = {
                  id: id,
                  amount: request.amount,
                  paymentDate: request.paymentDate,
                  capitalId: request.capitalId,
                  category: {
                    id: request.categoryId,
                    name: '',
                    type: CategoryType.Expenses,
                    totalExpenses: 0,
                    totalExpensesPercent: '0'
                  },
                  purpose: request.purpose
                };

                category = this.setCategoryById(response.category.id);

                if (category != null) {
                  category.totalExpenses += response.amount;
                  category.totalExpensesPercent = this.calculatePercent(category.totalExpenses);
                  response.category = category;

                  this.selectedCapital?.balance ? this.selectedCapital.balance -= response.amount : this.totalCapitalsBalance -= response.amount;
                  this.selectedCapital?.totalExpense ? this.selectedCapital.totalExpense += response.amount : this.totalCapitalsExpense += response.amount;
                  this.categories = this.constructCategories();
                }

                this.expenses.push(response);

                this.dialogService.close();
              }
            })
        }
      }
    });
  }

  onCapitalChange(capital: CapitalResponse | null): void {
    this.selectedCapital = capital;

    this.categories = this.constructCategories();
  }

  getCapitalBalance(): number {
    return this.selectedCapital?.balance ?? this.totalCapitalsBalance;
  }

  getCapitalTotalExpenses(): number {
    return this.selectedCapital?.totalExpense ?? this.totalCapitalsExpense;
  }

  hasExpenses(): boolean {
    return (this.selectedCapital?.totalExpense ?? this.totalCapitalsExpense) !== 0;
  }

  fetchExpenses(): void {
    this.expenseService
      .getAll(this.selectedCapital?.id)
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe({
        next: (response) => {
          this.expenses = response;
          this.categories = this.constructCategories();
        }
      });
  }

  fetchCapitals(): void {
    let capitalId: number | null = null;

    this.route.queryParams.subscribe({
      next: (params) => capitalId = params['capitalId']
    });

    this.capitalService
      .getAll()
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe({
        next: (response) => {
          this.capitals = response;
          this.selectedCapital = response.find(x => x.id == capitalId) ?? null;
          this.totalCapitalsBalance = this.calculateCapitalsBalance();
          this.totalCapitalsExpense = this.calculateCapitalsExpense();
        }
      });
  }

  fetchCategories(): void {
    this.categoryService
      .getAll(CategoryType.Expenses)
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe({
        next: (response) => this.dialogCategories = response
      })
  }

  private setCategoryById(categoryId: number): CategoryResponse | null {
    return this.categories.find(c => c.id == categoryId) ?? null;
  }

  private constructCategories(): CategoryResponse[] {
    let expenses: ExpenseResponse[] = this.expenses;

    if (this.selectedCapital?.id != null) {
      expenses = expenses.filter(e => e.capitalId == this.selectedCapital?.id);
    }

    const categoryTotals = expenses
      .reduce((map, expense) => {
        const { category, amount } = expense;

        if (!map.has(category.name)) {
          map.set(category.name, {
            ...category,
            totalExpenses: 0,
          });
        }

        const categoryEntry = map.get(category.name)!;

        categoryEntry.totalExpenses += amount;

        return map;
    }, new Map<string, CategoryResponse>());

    const totalExpenses = Array.from(categoryTotals.values()).reduce((sum, c) => sum + c.totalExpenses, 0);

    return Array.from(categoryTotals.values())
      .map((category) => {
        category.totalExpensesPercent = this.calculatePercent(totalExpenses); // TODO infinity bug
        return category;
      })
      .sort((a, b) => b.totalExpenses - a.totalExpenses);
  }

  private calculatePercent(sum: number): string {
    return Math.abs((sum / this.getCapitalBalance()) * 100).toFixed();
  }

  private calculateCapitalsBalance(): number {
    return this.capitals.reduce((sum, capital) => sum + capital.balance, 0);
  }

  private calculateCapitalsExpense(): number {
    return this.capitals.reduce((sum, capital) => sum + capital.totalExpense, 0);
  }

  protected readonly Periods = Periods;
}
