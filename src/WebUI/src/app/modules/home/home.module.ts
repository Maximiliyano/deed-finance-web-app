import { HomeComponent } from './home.component';
import { HomeRoutingModule } from './home-routing.module';
import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { CurrencyComponent } from '../../shared/components/currency/currency.component';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    HomeComponent
  ],
  imports: [HomeRoutingModule, SharedModule, CurrencyComponent, RouterModule, FormsModule]
})
export class HomeModule { }
