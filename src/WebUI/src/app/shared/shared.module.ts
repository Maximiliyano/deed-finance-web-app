import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { OverlayComponent } from './components/overlay/overlay.component';
import { LayoutComponent } from './components/layout/layout.component';
import { IconComponent } from './components/icon/icon.component';
import { FormErrorComponent } from './components/forms/form-error/form-error.component';
import { FormComponent } from './components/forms/form.component';
import { DatePickerComponent } from './components/date-picker/date-picker.component';
import { CurrencySymbolPipe } from './components/currency/pipes/currency-symbol-pipe';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { EnumTextPipe } from '../core/utils/enum';

@NgModule({
  declarations: [
    OverlayComponent,
    LayoutComponent,
    IconComponent,
    FormComponent,
    FormErrorComponent,
    DatePickerComponent,
    CurrencySymbolPipe,
    EnumTextPipe
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    DragDropModule,
  ],
  exports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    DragDropModule,

    LayoutComponent,
    IconComponent,
    FormComponent,
    FormErrorComponent,
    DatePickerComponent,
    CurrencySymbolPipe,
    EnumTextPipe
  ]
})
export class SharedModule { }
