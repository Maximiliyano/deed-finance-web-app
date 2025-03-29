import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { ExpenseRoutingModule } from './expense-routing.module';
import { ExpensesComponent } from './expenses.component';
import { ExpenseDetailsComponent } from './components/expense-details/expense-details.component';
import { ExpenseListComponent } from './components/expense-list/expense-list.component';
import { ExpenseDialogComponent } from './components/expense-dialog/expense-dialog.component';
import {DialogDatePickerComponent} from "./components/dialog-date-picker/dialog-date-picker.component";

@NgModule({
  declarations: [
    ExpensesComponent,
    ExpenseDialogComponent,
    ExpenseDetailsComponent,
    ExpenseListComponent,
    DialogDatePickerComponent
  ],
  imports: [ExpenseRoutingModule, SharedModule]
})
export class ExpenseModule { }
