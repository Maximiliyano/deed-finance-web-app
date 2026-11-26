import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
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
import { BankConnection } from '../../models/bank.model';
import { BankConnectionService } from '../../services/bank-connection.service';

@Component({
  selector: 'app-bank-connection-settings-dialog',
  templateUrl: './bank-connection-settings-dialog.component.html',
  styleUrl: './bank-connection-settings-dialog.component.scss',
  standalone: true,
  imports: [SharedModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BankConnectionSettingsDialogComponent implements OnInit {
  private readonly connection = inject<BankConnection>(DIALOG_DATA);
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject<DialogRef<boolean>>(DialogRef);
  private readonly bankConnection = inject(BankConnectionService);
  private readonly capitalService = inject(CapitalService);
  private readonly categoryService = inject(CategoryService);
  private readonly popup = inject(PopupMessageService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly form = this.fb.group({
    label: [this.connection.label, [Validators.maxLength(64)]],
    capitalId: [this.connection.capitalId],
    autoImport: [this.connection.autoImport],
    expenseCategoryId: [this.connection.expenseCategoryId],
    incomeCategoryId: [this.connection.incomeCategoryId]
  });

  protected readonly capitals = signal<CapitalResponse[]>([]);
  protected readonly expenseCategories = signal<CategoryResponse[]>([]);
  protected readonly incomeCategories = signal<CategoryResponse[]>([]);
  protected readonly loading = signal(true);
  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    forkJoin({
      capitals: this.capitalService.load({ searchTerm: null, sortBy: null, sortDirection: null, filterBy: null }),
      expenses: this.categoryService.load(CategoryType.Expenses),
      incomes: this.categoryService.load(CategoryType.Incomes)
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ capitals, expenses, incomes }) => {
          this.capitals.set(capitals);
          this.expenseCategories.set(expenses);
          this.incomeCategories.set(incomes);
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      });
  }

  protected get linked(): boolean {
    return this.form.controls.capitalId.value !== null;
  }

  protected get autoIncomplete(): boolean {
    const { autoImport, expenseCategoryId, incomeCategoryId } = this.form.getRawValue();

    return !!autoImport && (expenseCategoryId === null || incomeCategoryId === null);
  }

  protected submit(): void {
    if (this.form.invalid || this.autoIncomplete || this.submitting()) return;

    const { label, capitalId, autoImport, expenseCategoryId, incomeCategoryId } = this.form.getRawValue();
    const linked = capitalId !== null;

    this.submitting.set(true);
    this.error.set(null);

    this.bankConnection
      .update(this.connection.id, {
        label: label?.trim() || null,
        capitalId,
        autoImport: linked && !!autoImport,
        expenseCategoryId: linked ? expenseCategoryId : null,
        incomeCategoryId: linked ? incomeCategoryId : null
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.popup.success('Connection updated');
          this.dialogRef.close(true);
        },
        error: (response: HttpErrorResponse) => {
          this.submitting.set(false);
          this.error.set(response.error?.errors?.[0]?.message ?? 'Could not save the changes.');
        }
      });
  }

  protected cancel(): void {
    this.dialogRef.close(false);
  }
}
