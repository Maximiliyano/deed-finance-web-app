import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ExpensesComponent } from './expenses.component';
import { ExpenseListComponent } from './components/expense-list/expense-list.component';

const routes: Routes = [
  {
    path: '',
    component: ExpensesComponent,
    children: [
      {
        path: '',
        component: ExpenseListComponent
      }
    ]
  }
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ExpenseRoutingModule { }
