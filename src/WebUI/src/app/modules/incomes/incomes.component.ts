import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { IncomeService } from './services/income.service';
import { IncomeResponse, IncomeResponses } from './models/income-response';
import { Subject, takeUntil } from 'rxjs';
import { DialogService } from '../../shared/components/dialogs/services/dialog.service';
import { CreateIncomeDialogComponent } from './components/create-income-dialog.component/create-income-dialog.component';
import { SelectOptionModel } from '../../shared/components/forms/models/select-option-model';
import { PopupMessageService } from '../../shared/services/popup-message.service';
import { CreateIncomeRequest } from './models/create-income-request';

@Component({
  selector: 'app-incomes.component',
  imports: [],
  templateUrl: './incomes.component.html',
  styleUrl: './incomes.component.scss',
  standalone: true
})
export class IncomesComponent implements OnInit, OnDestroy {
  incomes = signal<IncomeResponse[]>([]);
  result = signal<IncomeResponses>({
    incomes: [],
    categories: [],
    capitals: []
  });

  private unsubscribe = new Subject<void>();

  constructor(
    private readonly incomeService: IncomeService,
    private readonly dialogService: DialogService,
    private readonly popup: PopupMessageService
  ){
    document.title = 'Deed - Incomes';
  }

  get capitalOptions(): SelectOptionModel[] {
    return this.result().capitals.map(x => { return { key: x.name, value: x.id } })
  }

  get categoryOptions(): SelectOptionModel[] {
    return this.result().categories.map(x => { return { key: x.name, value: x.id } });
  }

  ngOnInit(): void {
    this.incomeService
      .getAll()
      .pipe(takeUntil(this.unsubscribe))
      .subscribe((incomes) => {
        this.result.set(incomes);
      });
  }

  ngOnDestroy(): void {
    this.unsubscribe.next();
    this.unsubscribe.complete();
  }

  openCreateIncomeDialog(): void {
    const dialogRef = this.dialogService.open(CreateIncomeDialogComponent, {
      data: {
        capitalsOptions: this.capitalOptions,
        categoryOptions: this.categoryOptions
      }
    });
    dialogRef
      .afterClosed$
      .subscribe((request: CreateIncomeRequest) => {
        this.incomeService
          .create(request)
          .subscribe({
            next: (id: number) => {
              this.incomes.update((current) => [...current, {
                id: id,
                capitalId: request.capitalId,
                categoryId: request.categoryId,
                amount: request.amount,
                paymentDate: request.paymentDate,
                purpose: request.purpose
              }]);
              this.popup.success('Income created successfully.');
            }
        });
      });
  }
}
