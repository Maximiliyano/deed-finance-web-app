import { NgModule } from '@angular/core';
import { CapitalRoutingModule } from './capital-routing.module';
import { SharedModule } from '../../shared/shared.module';
import { CapitalDetailsComponent } from './components/capital-details/capital-details.component';
import { CapitalsComponent } from './capitals.component';
import { CapitalListComponent } from './components/capital-list/capital-list.component';
import { AddCapitalDialogComponent } from './components/capital-dialog/add-capital-dialog.component';

@NgModule({
  declarations: [
    CapitalsComponent,
    CapitalListComponent,
    CapitalDetailsComponent,
    AddCapitalDialogComponent
  ],
  imports: [CapitalRoutingModule, SharedModule]
})
export class CapitalModule { }
