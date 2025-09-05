import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { ExpenseRoutingModule } from './expense-routing.module';
import { ExpensesComponent } from './expenses.component';
import { ExpenseDialogComponent } from './components/expense-dialog/expense-dialog.component';
import { CategoriesDialogComponent } from './components/categories-dialog-component/categories-dialog-component';
import { DateTabPicker } from './components/date-tab-picker/date-tab-picker';

@NgModule({
  declarations: [
    ExpensesComponent,
    ExpenseDialogComponent,
    CategoriesDialogComponent,
    DateTabPicker
  ],
  imports: [ExpenseRoutingModule, SharedModule]
})
export class ExpenseModule { }
