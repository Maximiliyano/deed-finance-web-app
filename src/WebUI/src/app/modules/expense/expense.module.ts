import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { ExpenseRoutingModule } from './expense-routing.module';
import { ExpensesComponent } from './expenses.component';

@NgModule({
  declarations: [
    ExpensesComponent
  ],
  imports: [ExpenseRoutingModule, SharedModule]
})
export class ExpenseModule { }
