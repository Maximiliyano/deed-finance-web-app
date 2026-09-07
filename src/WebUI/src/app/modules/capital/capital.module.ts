import { NgModule } from '@angular/core';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { CapitalRoutingModule } from './capital-routing.module';
import { SharedModule } from '../../shared/shared.module';
import { CapitalsComponent } from './capitals.component';
import { FormComponent } from "../../shared/components/forms/form.component";

@NgModule({
  declarations: [
    CapitalsComponent
  ],
  imports: [CapitalRoutingModule, SharedModule, FormComponent, DragDropModule]
})
export class CapitalModule { }
