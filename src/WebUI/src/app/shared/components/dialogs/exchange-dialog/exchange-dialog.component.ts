import { Component } from '@angular/core';
import { Exchange } from '../../../../core/models/exchange-model';

@Component({
    selector: 'app-exchange-dialog',
    templateUrl: './exchange-dialog.component.html',
    styleUrl: './exchange-dialog.component.scss',
    standalone: false
})
export class ExchangeDialogComponent {
  exchanges: Exchange[] = [];
}
