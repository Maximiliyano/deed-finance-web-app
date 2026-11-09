import { NgModule } from '@angular/core';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { CapitalRoutingModule } from './capital-routing.module';
import { SharedModule } from '../../shared/shared.module';
import { CapitalsComponent } from './capitals.component';

@NgModule({
  declarations: [
    CapitalsComponent
  ],
  imports: [CapitalRoutingModule, SharedModule, DragDropModule]
})
export class CapitalModule { }
