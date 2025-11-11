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
    path: 'error',
    loadComponent: () =>
      import('./core/layout/error/error.component').then((x) => x.ErrorComponent)
  },
  {
    path: '**',
    redirectTo: '/error',
    pathMatch: 'full',
    data: { status: 404 } 
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { scrollPositionRestoration: 'enabled' })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
