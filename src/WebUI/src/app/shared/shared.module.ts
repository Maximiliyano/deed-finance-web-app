import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { OverlayComponent } from './components/overlay/overlay.component';
import { LayoutComponent } from './components/layout/layout.component';
import { CurrencySymbolPipe } from './components/currency/pipes/currency-symbol-pipe';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { EnumTextPipe } from '../core/utils/enum';
import { IconComponent } from './components/icon/icon.component';

@NgModule({
  declarations: [
    LayoutComponent,
    CurrencySymbolPipe,
    EnumTextPipe
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    DragDropModule,

    IconComponent
],
  exports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    DragDropModule,

    LayoutComponent,
    IconComponent,
    CurrencySymbolPipe,
    EnumTextPipe
  ]
})
export class SharedModule { }
