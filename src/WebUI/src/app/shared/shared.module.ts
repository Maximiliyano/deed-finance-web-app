import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { LayoutComponent } from './components/layout/layout.component';
import { CurrencySymbolPipe } from './components/currency/pipes/currency-symbol-pipe';
import { EnumTextPipe } from '../core/utils/enum';
import { IconComponent } from './components/icon/icon.component';
import { NumericInputDirective } from './directives/numeric-input.directive';
import { CountUpDirective } from './directives/count-up.directive';

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
    IconComponent,
    NumericInputDirective,
    CountUpDirective
],
  exports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    LayoutComponent,
    IconComponent,
    NumericInputDirective,
    CountUpDirective,
    CurrencySymbolPipe,
    EnumTextPipe
  ]
})
export class SharedModule { }
