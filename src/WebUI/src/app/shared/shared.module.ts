import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { OverlayComponent } from './components/overlay/overlay.component';
import { ConfirmDialogComponent } from './components/dialogs/confirm-dialog/confirm-dialog.component';
import { LayoutComponent } from './components/layout/layout.component';
import { ExchangeDialogComponent } from './components/dialogs/exchange-dialog/exchange-dialog.component';
import { IconComponent } from './components/icon/icon.component';
import { DialogComponent } from './components/dialogs/dialog.component';
import { FormErrorComponent } from './components/forms/form-error/form-error.component';
import { FormComponent } from './components/forms/form.component';
import { DatePickerComponent } from './components/date-picker/date-picker.component';

@NgModule({
  declarations: [
    OverlayComponent,
    ConfirmDialogComponent,
    ExchangeDialogComponent,
    LayoutComponent,
    IconComponent,
    DialogComponent,
    FormComponent,
    FormErrorComponent,
    DatePickerComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule
  ],
  exports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    LayoutComponent,
    IconComponent,
    DialogComponent,
    FormComponent,
    FormErrorComponent,
    DatePickerComponent
  ]
})
export class SharedModule { }
