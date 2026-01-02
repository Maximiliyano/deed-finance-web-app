import { NgModule } from '@angular/core';
import { CapitalRoutingModule } from './capital-routing.module';
import { SharedModule } from '../../shared/shared.module';
import { CapitalsComponent } from './capitals.component';
import { FormComponent } from "../../shared/components/forms/form.component";

@NgModule({
  declarations: [
    CapitalsComponent
  ],
  imports: [CapitalRoutingModule, SharedModule, FormComponent]
})
export class CapitalModule { }
