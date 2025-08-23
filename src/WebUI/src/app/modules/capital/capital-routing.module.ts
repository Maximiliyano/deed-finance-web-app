import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CapitalsComponent } from './capitals.component';

const routes: Routes = [
  {
    path: '',
    component: CapitalsComponent,
  }
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class CapitalRoutingModule { }
