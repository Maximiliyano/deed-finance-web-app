import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '',
    loadChildren: () =>
      import('./modules/home/home.module').then((x) => x.HomeModule)
  },
  {
    path: 'capitals',
    loadChildren: () =>
      import('./modules/capital/capital.module').then((x) => x.CapitalModule)
  },
  {
    path: 'expenses',
    loadChildren: () =>
      import('./modules/expense/expense.module').then((x) => x.ExpenseModule)
  },
  {
    path: '404',
    pathMatch: 'full',
    loadComponent: () =>
      import('./core/layout/not-found/not-found.component').then((x) => x.NotFoundComponent)
  },
  {
    path: '**',
    redirectTo: '404'
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
