import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home.component';
import { SocialListComponent } from './social-list/social-list.component';

const routes: Routes = [
  {
    path: '',
    component: HomeComponent,
    children: [
      {
        component: SocialListComponent,
        path: ''
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class HomeRoutingModule { }
