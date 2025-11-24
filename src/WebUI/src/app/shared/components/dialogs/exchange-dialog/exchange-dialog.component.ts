import { Component, Inject } from '@angular/core';
import { Exchange } from '../../../../core/models/exchange-model';
import { DecimalPipe } from '@angular/common';
import { DIALOG_DATA } from '../models/dialog-consts';

@Component({
    selector: 'app-exchange-dialog',
    templateUrl: './exchange-dialog.component.html',
    styleUrl: './exchange-dialog.component.scss',
    imports: [DecimalPipe],
    standalone: true
})
export class ExchangeDialogComponent {
  constructor(
    @Inject(DIALOG_DATA) public exchanges: Exchange[]
  ) {}
}
