import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, signal } from '@angular/core';
import { IncomeService } from './services/income.service';
import { IncomeResponse } from './models/income-response';
import { Subject, takeUntil } from 'rxjs';
import { DialogService } from '../../shared/components/dialogs/services/dialog.service';
import { CreateIncomeDialogComponent } from './components/create-income-dialog.component/create-income-dialog.component';
import { SelectOptionModel } from '../../shared/components/forms/models/select-option-model';
import { PopupMessageService } from '../../shared/services/popup-message.service';
import { CreateIncomeRequest } from './models/create-income-request';
import { CategoryResponse } from '../category/models/category-model';
import { CapitalResponse } from '../capital/models/capital-response';

@Component({
  selector: 'app-incomes.component',
  imports: [CommonModule],
  templateUrl: './incomes.component.html',
  styleUrl: './incomes.component.scss',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class IncomesComponent implements OnInit, OnDestroy {
  incomes = signal<IncomeResponse[]>([]);
  categories = signal<CategoryResponse[]>([]);
  capitals = signal<CapitalResponse[]>([]);

  private unsubscribe = new Subject<void>();

  constructor(
    private readonly incomeService: IncomeService,
    private readonly dialogService: DialogService,
    private readonly popup: PopupMessageService
  ){
    document.title = 'Deed - Incomes';
  }

  get capitalOptions(): SelectOptionModel[] {
    return this.capitals().map(x => ({ key: x.name, value: x.id }));
  }

  get categoryOptions(): SelectOptionModel[] {
    return this.categories().map(x => ({ key: x.name, value: x.id }));
  }

  onDelete(id: number) {
    this.incomeService.delete(id).pipe(takeUntil(this.unsubscribe)).subscribe({
      next: () => this.popup.success('Removed succ')
    });
  }

  capitalName(id: number | null): string {
    if (!id) return '—';
    return this.capitals().find(c => c.id === id)?.name ?? '—';
  }

  categoryName(id: number | null): string {
    if (!id) return '—';
    return this.categories().find(c => c.id === id)?.name ?? '—';
  }

  ngOnInit(): void {
    this.incomeService
      .getAll()
      .pipe(takeUntil(this.unsubscribe))
      .subscribe(incomes => this.incomes.set(incomes));
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
      .pipe(takeUntil(this.unsubscribe))
      .subscribe((request: CreateIncomeRequest) => {
        if (!request) return;
        this.incomeService
          .create(request)
          .pipe(takeUntil(this.unsubscribe))
          .subscribe({
            next: (id: number) => {
              this.incomes.update(incomes => [...incomes, {
                id,
                capitalId:   request.capitalId,
                categoryId:  request.categoryId,
                amount:      request.amount,
                paymentDate: request.paymentDate,
                purpose:     request.purpose
              }]);
              this.popup.success('Income created successfully.');
            }
          });
      });
  }
}
