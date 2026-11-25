import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { ExpenseRoutingModule } from './expense-routing.module';
import { ExpensesComponent } from './expenses.component';
import { UtilityBillListComponent } from './components/utility-bill-list/utility-bill-list.component';

@NgModule({
  declarations: [ExpensesComponent, UtilityBillListComponent],
  imports: [ExpenseRoutingModule, SharedModule]
})
export class ExpenseModule {}
