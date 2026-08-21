import { HomeComponent } from './home.component';
import { HomeRoutingModule } from './home-routing.module';
import { NgModule } from '@angular/core';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { SharedModule } from '../../shared/shared.module';
import { CurrencyComponent } from '../../shared/components/currency/currency.component';
import { RouterModule } from '@angular/router';
import { FinanceDashboardComponent } from "./components/finance-dashboard/finance-dashboard.component";
@NgModule({
  declarations: [
    HomeComponent
  ],
  imports: [HomeRoutingModule, SharedModule, CurrencyComponent, RouterModule, DragDropModule, FinanceDashboardComponent]
})
export class HomeModule { }
