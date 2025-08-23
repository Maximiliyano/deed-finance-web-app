import { NgModule } from '@angular/core';
import { CapitalRoutingModule } from './capital-routing.module';
import { SharedModule } from '../../shared/shared.module';
import { CapitalsComponent } from './capitals.component';
import { AddCapitalDialogComponent } from './components/capital-dialog/add-capital-dialog.component';
import { CapitalDetailsComponent } from './components/capital-details/capital-details.component';

@NgModule({
  declarations: [
    CapitalsComponent,
    CapitalDetailsComponent,
    AddCapitalDialogComponent
  ],
  imports: [CapitalRoutingModule, SharedModule]
})
export class CapitalModule { }
