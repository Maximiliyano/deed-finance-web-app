import { PopupMessageService } from './../../../services/popup-message.service';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Exchange } from '../../../../core/models/exchange-model';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DIALOG_DATA } from '../models/dialog-consts';
import { convertCurrency } from '../../../utils/currency-conversion.util';
import { IconComponent } from "../../icon/icon.component";
import { ClipboardModule } from '@angular/cdk/clipboard'
import { SectionKey, SectionLoadingService } from '../../../services/section-loading.service';

import { CURRENCIES } from '../../../../core/types/currency-type';

@Component({
    selector: 'app-exchange-dialog',
    templateUrl: './exchange-dialog.component.html',
    styleUrl: './exchange-dialog.component.scss',
    imports: [DecimalPipe, FormsModule, IconComponent, ClipboardModule, DatePipe],
    standalone: true,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExchangeDialogComponent implements OnInit {
  private readonly popupMessageService = inject(PopupMessageService);
  private readonly sectionLoading = inject(SectionLoadingService);
  private readonly cdr = inject(ChangeDetectorRef);
  readonly exchanges = inject<Exchange[]>(DIALOG_DATA);
  private readonly destroyRef = inject(DestroyRef);

  amount = 1;
  fromCurrency: string;
  toCurrency: string;

  readonly currencies: string[] = CURRENCIES.map(x => x);

  ngOnInit(): void {
    this.sectionLoading.isLoading$('exchanges')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        const base = this.exchanges[0]?.nationalCurrency ?? 'UAH';
        const first = this.exchanges[0]?.targetCurrency ?? 'USD';

        this.fromCurrency = first;
        this.toCurrency = base;
        this.cdr.markForCheck()
      });
  }

  get result(): number {
    return convertCurrency(this.amount || 0, this.fromCurrency, this.toCurrency, this.exchanges);
  }

  swap(): void {
    [this.fromCurrency, this.toCurrency] = [this.toCurrency, this.fromCurrency];
  }

  onCopied(copied: boolean) {
    if (copied) {
      this.popupMessageService.info('Message is copied to the clipboard.');
    }
  }

  loading(key: SectionKey): boolean {
    return this.sectionLoading.isLoading(key);
  }
}
