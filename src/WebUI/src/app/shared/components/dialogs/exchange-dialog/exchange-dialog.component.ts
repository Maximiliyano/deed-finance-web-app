import { PopupMessageService } from './../../../services/popup-message.service';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit } from '@angular/core';
import { Exchange } from '../../../../core/models/exchange-model';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DIALOG_DATA } from '../models/dialog-consts';
import { getCurrencies } from '../../currency/functions/get-currencies.component';
import { convertCurrency } from '../../../utils/currency-conversion.util';
import { IconComponent } from "../../icon/icon.component";
import { ClipboardModule } from '@angular/cdk/clipboard'
import { SectionKey, SectionLoadingService } from '../../../services/section-loading.service';
import { Subject, takeUntil } from 'rxjs';

@Component({
    selector: 'app-exchange-dialog',
    templateUrl: './exchange-dialog.component.html',
    styleUrl: './exchange-dialog.component.scss',
    imports: [DecimalPipe, FormsModule, IconComponent, ClipboardModule],
    standalone: true,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExchangeDialogComponent implements OnInit {
  amount = 1;
  fromCurrency: string;
  toCurrency: string;

  readonly currencies: string[] = getCurrencies({ excludeNone: true }).map(c => c.key);

  constructor(
    private readonly popupMessageService: PopupMessageService,
    private readonly sectionLoading: SectionLoadingService,
    private readonly cdr: ChangeDetectorRef,
    @Inject(DIALOG_DATA) public exchanges: Exchange[]
  ) {}

  private unsubscribe$ = new Subject<void>();

  ngOnInit(): void {
    this.sectionLoading.isLoading$('exchanges')
      .pipe(takeUntil(this.unsubscribe$))
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
