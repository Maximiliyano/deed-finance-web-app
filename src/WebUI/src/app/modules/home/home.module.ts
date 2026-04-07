import { HomeComponent } from './home.component';
import { HomeRoutingModule } from './home-routing.module';
import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { CurrencyComponent } from '../../shared/components/currency/currency.component';
import { RouterModule } from '@angular/router';
@NgModule({
  declarations: [
    HomeComponent
  ],
  imports: [HomeRoutingModule, SharedModule, CurrencyComponent, RouterModule]
})
export class HomeModule { }
