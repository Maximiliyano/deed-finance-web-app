import { Component, inject, input, OnInit } from '@angular/core';
import { CapitalService } from '../../../capital/services/capital.service';
import { ExchangeService } from '../../../../shared/services/exchange.service';
import { Exchange } from '../../../../core/models/exchange-model';
import { convertCurrency } from '../../../../shared/utils/currency-conversion.util';
import { CurrencyType } from '../../../../core/types/currency-type';
import { IncomeService } from '../../../incomes/services/income.service';
import { ExpenseService } from '../../../expense/services/expense.service';
import { SharedModule } from "../../../../shared/shared.module";

@Component({
  selector: 'app-finance-dashboard',
  templateUrl: './finance-dashboard.component.html',
  styleUrl: './finance-dashboard.component.scss',
  standalone: true,
  imports: [SharedModule]
})
export class FinanceDashboardComponent implements OnInit {
  currency = input<string>('');

  private readonly capitalService = inject(CapitalService);
  private readonly incomeService = inject(IncomeService);
  private readonly expenseService = inject(ExpenseService);
  private readonly exchangeService = inject(ExchangeService);

  planFilter: 'day' | 'month' | 'year' = 'month';

  private exchanges: Exchange[];

  constructor()
  { }

  ngOnInit(): void {
    this.exchangeService.getLatest().subscribe({
      next: (exchanges) => this.exchanges = exchanges
    });
  }

  private get planFactor(): number {
    switch (this.planFilter) {
      case 'day':  return 1 / 30;
      case 'year': return 12;
      default:     return 1;
    }
  }

  get planFilterLabel(): string {
    return this.planFilter === 'day' ? 'Daily' : this.planFilter === 'year' ? 'Yearly' : 'Monthly';
  }

  get profitPerTimeInUah(): number {
    if (!this.showDualCurrency) {
      return this.profitPerTime;
    }

    return convertCurrency(this.profitPerTime, this.currency(), CurrencyType.UAH.toString(), this.exchanges);
  }

  get profitPerTime(): number {
    return this.totalProfit * this.planFactor;
  }

  get totalProfit(): number {
    return this.totalIncomeAmount - this.totalExpenseAmount;
  }

  get isProfitable(): boolean {
    return this.totalProfit > 0;
  }

  get showDualCurrency(): boolean {
    return this.currency() !== 'UAH';
  }

  get totalExpenseAmount(): number {
    let sum = 0;

    for (const expenseCategoryResponse of this.expenseService.objects) {
      for (const e of expenseCategoryResponse.expenses) {
        sum += convertCurrency(e.amount, this.capitalService.getCurrency(e.capitalId) ?? CurrencyType.None.toString(), this.currency(), this.exchanges);
      }
    }

    return sum;
  }

  get totalExpenseAmountInUah(): number {
    if (!this.showDualCurrency) {
      return this.totalExpenseAmount;
    }

    return convertCurrency(this.totalExpenseAmount, this.currency(), CurrencyType.UAH.toString(), this.exchanges);
  }

  get totalIncomeAmount(): number {
    return this.incomeService.currentIncomes.length > 0 ? this.incomeService.currentIncomes
      .reduce(
        (sum, i) => sum + convertCurrency(i.amount, this.capitalService.getCurrency(i.capitalId) ?? CurrencyType.None.toString(), this.currency(), this.exchanges), 0
      ) : 0;
  }

  get totalIncomeAmountInUah(): number {
    if (!this.showDualCurrency) {
      return this.totalIncomeAmount;
    }

    return convertCurrency(this.totalIncomeAmount, this.currency(), CurrencyType.UAH.toString(), this.exchanges);
  }

  get totalCapitalAmount(): number {
    return this.capitalService.getTotalAmount(this.currency(), this.exchanges);
  }

  get totalCapitalAmountInUah(): number {
    if (!this.showDualCurrency) {
      return this.totalCapitalAmount;
    }

    return convertCurrency(this.totalCapitalAmount, this.currency(), CurrencyType.UAH.toString(), this.exchanges );
  }
}
