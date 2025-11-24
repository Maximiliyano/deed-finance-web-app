import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { ExpenseRoutingModule } from './expense-routing.module';
import { ExpensesComponent } from './expenses.component';
import { DateTabPicker } from './components/date-tab-picker/date-tab-picker';

@NgModule({
  declarations: [
    ExpensesComponent,
    DateTabPicker
  ],
  imports: [ExpenseRoutingModule, SharedModule]
})
export class ExpenseModule { }
