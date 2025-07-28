import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AngularMaterialModule } from './angular-material.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { OverlayComponent } from './components/overlay/overlay.component';
import { ConfirmDialogComponent } from './components/dialogs/confirm-dialog/confirm-dialog.component';
import { CurrencyComponent } from './components/currency/currency.component';
import { LayoutComponent } from './components/layout/layout.component';
import { ExchangeDialogComponent } from './components/dialogs/exchange-dialog/exchange-dialog.component';
import { IconComponent } from './components/icon/icon.component';

@NgModule({
  declarations: [
    OverlayComponent,
    ConfirmDialogComponent,
    ExchangeDialogComponent,
    CurrencyComponent,
    LayoutComponent,
    IconComponent
  ],
  imports: [
    CommonModule,
    AngularMaterialModule,
    FormsModule,
    ReactiveFormsModule
  ],
  exports: [
    CommonModule,
    AngularMaterialModule,
    FormsModule,
    ReactiveFormsModule,
    CurrencyComponent,
    LayoutComponent,
    IconComponent
  ]
})
export class SharedModule { }
