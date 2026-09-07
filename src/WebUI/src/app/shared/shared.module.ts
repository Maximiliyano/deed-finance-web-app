import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { OverlayComponent } from './components/overlay/overlay.component';
import { LayoutComponent } from './components/layout/layout.component';
import { CurrencySymbolPipe } from './components/currency/pipes/currency-symbol-pipe';
import { EnumTextPipe } from '../core/utils/enum';
import { IconComponent } from './components/icon/icon.component';

@NgModule({
  declarations: [
    OverlayComponent,
    LayoutComponent,
    CurrencySymbolPipe,
    EnumTextPipe
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IconComponent
],
  exports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    LayoutComponent,
    IconComponent,
    CurrencySymbolPipe,
    EnumTextPipe
  ]
})
export class SharedModule { }
