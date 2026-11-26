import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./modules/home/home.component').then(x => x.HomeComponent)
  },
  {
    path: 'bank',
    loadComponent: () =>
      import('./modules/bank/components/bank-transactions/bank-transactions.component').then(
        x => x.BankTransactionsComponent
      )
  },
  {
    path: 'error',
    loadComponent: () => import('./core/layout/error/error.component').then(x => x.ErrorComponent)
  },
  {
    path: '**',
    redirectTo: '/error',
    pathMatch: 'full'
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { scrollPositionRestoration: 'enabled' })],
  exports: [RouterModule]
})
export class AppRoutingModule {}
