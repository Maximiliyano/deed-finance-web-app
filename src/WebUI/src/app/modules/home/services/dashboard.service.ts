import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';
import {environment} from '../../../../environments/environment';
import {UserSettings} from '../models/user-settings.model';
import {BudgetEstimation} from '../models/budget-estimation.model';
import {Goal} from '../models/goal.model';
import {Debt} from '../models/debt.model';
import {CapitalResponse} from '../../capital/models/capital-response';
import {Exchange} from '../../../core/models/exchange-model';
import {ExpenseCategoryResponse} from '../../expense/models/expense-category-response';
import {IncomeResponse} from '../../incomes/models/income-response';
import {CategoryResponse} from '../../category/models/category-model';
import {TransferResponse} from '../../../shared/services/transfer.service';

export interface DashboardData {
  settings: UserSettings | null;
  capitals: CapitalResponse[];
  exchanges: Exchange[];
  estimations: BudgetEstimation[];
  goals: Goal[];
  debts: Debt[];
  expenseCategories: ExpenseCategoryResponse[];
  incomes: IncomeResponse[];
  incomeCategories: CategoryResponse[];
  expenseCategoriesList: CategoryResponse[];
  transfers: TransferResponse[];
}

@Injectable({providedIn: 'root'})
export class DashboardService {
  private readonly baseUrl = `${environment.apiUrl}/api/dashboard`;

  constructor(private readonly http: HttpClient) {
  }

  get(): Observable<DashboardData> {
    return this.http.get<DashboardData>(this.baseUrl, {withCredentials: true});
  }
}
