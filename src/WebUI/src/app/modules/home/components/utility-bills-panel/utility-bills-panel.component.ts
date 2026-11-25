import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { EMPTY, Observable, catchError, finalize, forkJoin, of } from 'rxjs';
import { CategoryType } from '../../../../core/types/category-type';
import { ConfirmDialogComponent } from '../../../../shared/components/dialogs/confirm-dialog/confirm-dialog.component';
import { DialogService } from '../../../../shared/components/dialogs/services/dialog.service';
import { PanelStateComponent } from '../../../../shared/components/panel-state/panel-state.component';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { SharedModule } from '../../../../shared/shared.module';
import { CapitalService } from '../../../capital/services/capital.service';
import { CategoryResponse } from '../../../category/models/category-model';
import { CategoryService } from '../../../category/services/category.service';
import {
  PayUtilityBillDialogComponent,
  PayUtilityBillDialogData
} from '../../../expense/components/pay-utility-bill-dialog/pay-utility-bill-dialog.component';
import {
  UtilityBillDialogComponent,
  UtilityBillDialogData
} from '../../../expense/components/utility-bill-dialog/utility-bill-dialog.component';
import {
  CreateUtilityBillRequest,
  PayUtilityBillRequest,
  UtilityBillResponse
} from '../../../expense/models/utility-bill';
import { ExpenseService } from '../../../expense/services/expense.service';
import { UtilityBillService } from '../../../expense/services/utility-bill.service';
import { DashboardStore } from '../../services/dashboard-store.service';

type BillStatus = 'overdue' | 'soon' | 'upcoming' | 'paid' | 'inactive';

interface BillRow {
  bill: UtilityBillResponse;
  status: BillStatus;
  view: { label: string; badge: string };
}

const STATUS_VIEW: Record<BillStatus, { label: string; badge: string }> = {
  overdue: { label: 'Overdue', badge: 'badge-danger' },
  soon: { label: 'Due soon', badge: 'badge-warning' },
  upcoming: { label: 'Upcoming', badge: 'badge-neutral' },
  paid: { label: 'Paid', badge: 'badge-success' },
  inactive: { label: 'Inactive', badge: 'badge-neutral' }
};

const STATUS_ORDER: Record<BillStatus, number> = { overdue: 0, soon: 1, upcoming: 2, paid: 3, inactive: 4 };
const DUE_SOON_DAYS = 3;

@Component({
  selector: 'app-utility-bills-panel',
  templateUrl: './utility-bills-panel.component.html',
  styleUrl: './utility-bills-panel.component.scss',
  standalone: true,
  imports: [SharedModule, PanelStateComponent],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UtilityBillsPanelComponent implements OnInit {
  readonly store = inject(DashboardStore);
  private readonly billService = inject(UtilityBillService);
  private readonly categoryService = inject(CategoryService);
  private readonly capitalService = inject(CapitalService);
  private readonly expenseService = inject(ExpenseService);
  private readonly dialogService = inject(DialogService);
  private readonly popup = inject(PopupMessageService);
  private readonly destroyRef = inject(DestroyRef);

  private readonly bills = signal<UtilityBillResponse[]>([]);
  private readonly categories = signal<CategoryResponse[]>([]);
  private readonly loaded = signal(false);

  readonly isLoading = computed(() => !this.loaded());

  readonly rows = computed<BillRow[]>(() =>
    this.bills()
      .map(bill => {
        const status = this.statusOf(bill);
        return { bill, status, view: STATUS_VIEW[status] };
      })
      .sort((a, b) => STATUS_ORDER[a.status] - STATUS_ORDER[b.status] || a.bill.dueDayOfMonth - b.bill.dueDayOfMonth)
  );

  readonly unpaidCount = computed(
    () => this.rows().filter(row => row.status !== 'paid' && row.status !== 'inactive').length
  );

  ngOnInit(): void {
    forkJoin([
      this.billService.getAll().pipe(catchError(() => of([] as UtilityBillResponse[]))),
      this.categoryService.load(CategoryType.Expenses).pipe(catchError(() => of([] as CategoryResponse[])))
    ])
      .pipe(
        finalize(() => this.loaded.set(true)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(([bills, categories]) => {
        this.bills.set(bills);
        this.categories.set(categories);
      });
  }

  openDialog(bill?: UtilityBillResponse): void {
    const data: UtilityBillDialogData = {
      bill,
      capitals: this.store.capitals().filter(capital => !capital.onlyForSavings),
      categories: this.categories()
    };
    const ref = this.dialogService.open(UtilityBillDialogComponent, { data });

    ref.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((result: CreateUtilityBillRequest | null) => {
      if (!result) return;

      const save$: Observable<unknown> = bill
        ? this.billService.update(bill.id, result)
        : this.billService.create(result);
      save$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
        this.popup.success(bill ? 'Utility bill updated' : 'Utility bill added');
        this.reload();
      });
    });
  }

  pay(bill: UtilityBillResponse): void {
    const data: PayUtilityBillDialogData = { bill };
    const ref = this.dialogService.open(PayUtilityBillDialogComponent, { data });

    ref.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((result: PayUtilityBillRequest | null) => {
      if (!result) return;

      this.billService
        .pay(bill.id, result)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(() => {
          this.popup.success(`${bill.name} marked as paid`);
          this.reload();
          this.capitalService.refresh();
          this.expenseService
            .load()
            .pipe(
              catchError(() => EMPTY),
              takeUntilDestroyed(this.destroyRef)
            )
            .subscribe();
        });
    });
  }

  delete(bill: UtilityBillResponse): void {
    const ref = this.dialogService.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete utility bill',
        message: `Delete "${bill.name}"? Expenses already created from it stay as they are.`,
        icon: 'danger'
      }
    });

    ref.afterClosed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((confirmed: boolean) => {
      if (!confirmed) return;

      this.billService
        .delete(bill.id)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(() => {
          this.popup.success('Utility bill deleted');
          this.reload();
        });
    });
  }

  private reload(): void {
    this.billService
      .getAll()
      .pipe(
        catchError(() => EMPTY),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(bills => this.bills.set(bills));
  }

  private statusOf(bill: UtilityBillResponse): BillStatus {
    if (bill.isPaidThisMonth) return 'paid';
    if (!bill.isActive) return 'inactive';

    const today = new Date().getDate();
    if (bill.dueDayOfMonth < today) return 'overdue';
    if (bill.dueDayOfMonth - today <= DUE_SOON_DAYS) return 'soon';
    return 'upcoming';
  }
}
