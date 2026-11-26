import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { CategoryType } from '../../../../core/types/category-type';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { SharedModule } from '../../../../shared/shared.module';
import { CapitalResponse } from '../../../capital/models/capital-response';
import { CapitalService } from '../../../capital/services/capital.service';
import { CategoryResponse } from '../../../category/models/category-model';
import { CategoryService } from '../../../category/services/category.service';
import { BankConnection, BankNotification } from '../../models/bank.model';
import { BankTransactionService } from '../../services/bank-transaction.service';

export interface ImportTransactionDialogData {
  notification: BankNotification;
  connection: BankConnection | undefined;
}

@Component({
  selector: 'app-import-transaction-dialog',
  templateUrl: './import-transaction-dialog.component.html',
  styleUrl: './import-transaction-dialog.component.scss',
  standalone: true,
  imports: [SharedModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ImportTransactionDialogComponent implements OnInit {
  protected readonly data = inject<ImportTransactionDialogData>(DIALOG_DATA);
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject<DialogRef<boolean>>(DialogRef);
  private readonly transactions = inject(BankTransactionService);
  private readonly capitalService = inject(CapitalService);
  private readonly categoryService = inject(CategoryService);
  private readonly popup = inject(PopupMessageService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly outgoing = this.data.notification.direction === 'Outgoing';

  private readonly defaultCategoryId =
    (this.outgoing ? this.data.connection?.expenseCategoryId : this.data.connection?.incomeCategoryId) ?? null;

  protected readonly form = this.fb.group({
    categoryId: [this.defaultCategoryId as number | null, [Validators.required]]
  });

  protected readonly capitals = signal<CapitalResponse[]>([]);
  protected readonly categories = signal<CategoryResponse[]>([]);
  protected readonly loading = signal(true);
  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly capital = computed(() => this.capitals().find(c => c.id === this.data.connection?.capitalId));

  ngOnInit(): void {
    forkJoin({
      capitals: this.capitalService.load({ searchTerm: null, sortBy: null, sortDirection: null, filterBy: null }),
      categories: this.categoryService.load(this.outgoing ? CategoryType.Expenses : CategoryType.Incomes)
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ capitals, categories }) => {
          this.capitals.set(capitals);
          this.categories.set(categories);
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      });
  }

  protected submit(): void {
    const categoryId = this.form.controls.categoryId.value;

    if (this.form.invalid || categoryId === null || this.submitting()) return;

    this.submitting.set(true);
    this.error.set(null);

    this.transactions
      .addToCapital(this.data.notification.id, categoryId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.popup.success(this.outgoing ? 'Expense added' : 'Income added');
          this.dialogRef.close(true);
        },
        error: (response: HttpErrorResponse) => {
          this.submitting.set(false);
          this.error.set(response.error?.errors?.[0]?.message ?? 'Could not add the transaction.');
        }
      });
  }

  protected cancel(): void {
    this.dialogRef.close(false);
  }
}
