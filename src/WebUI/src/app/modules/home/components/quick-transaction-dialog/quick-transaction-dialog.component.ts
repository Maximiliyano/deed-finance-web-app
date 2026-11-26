import {
  AfterViewInit,
  ChangeDetectorRef,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  OnInit,
  inject
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { NumericInputDirective } from '../../../../shared/directives/numeric-input.directive';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { CapitalResponse } from '../../../capital/models/capital-response';
import { CategoryResponse } from '../../../category/models/category-model';
import { CategoryType } from '../../../../core/types/category-type';
import { CategoryService } from '../../../category/services/category.service';
import { PerPeriodType } from '../../../../core/types/per-period-type';

export interface QuickTransactionDialogData {
  type: 'expense' | 'income';
  capitals: CapitalResponse[];
  categories: CategoryResponse[];
  editItem?: {
    id: number;
    capitalId: number;
    categoryId: number;
    amount: number;
    paymentDate: string;
    purpose: string | null;
    tagNames?: string[] | null;
  };
  defaults?: { capitalId?: number; expenseCategoryId?: number; incomeCategoryId?: number };
}

@Component({
  selector: 'app-quick-transaction-dialog',
  templateUrl: './quick-transaction-dialog.component.html',
  styleUrl: './quick-transaction-dialog.component.scss',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, NumericInputDirective],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class QuickTransactionDialogComponent implements OnInit, AfterViewInit {
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject<DialogRef<any>>(DialogRef);
  private readonly categoryService = inject(CategoryService);
  private readonly data = inject<QuickTransactionDialogData>(DIALOG_DATA);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  form!: FormGroup;
  type: 'expense' | 'income' = this.data.type;
  capitals: CapitalResponse[] = this.data.capitals;
  categories: CategoryResponse[] = this.data.categories.filter(c => !c.isDeleted);
  showNewCategory = false;
  newCategoryName = '';
  isEdit = false;
  editId: number | null = null;

  ngOnInit(): void {
    const e = this.data.editItem;
    this.isEdit = !!e;
    this.editId = e?.id ?? null;

    const defaults = this.data.defaults;
    const defaultCapital =
      defaults?.capitalId && this.capitals.some(c => c.id === defaults.capitalId)
        ? defaults.capitalId
        : this.capitals.length === 1
          ? this.capitals[0].id
          : null;
    const rememberedCategory = this.isExpense ? defaults?.expenseCategoryId : defaults?.incomeCategoryId;
    const defaultCategory =
      rememberedCategory && this.categories.some(c => c.id === rememberedCategory) ? rememberedCategory : null;

    this.form = this.fb.group({
      capitalId: [e?.capitalId ?? defaultCapital, Validators.required],
      categoryId: [e?.categoryId ?? defaultCategory, Validators.required],
      amount: [e?.amount ?? null, [Validators.required, Validators.min(0.01)]],
      paymentDate: [
        e?.paymentDate ? new Date(e.paymentDate).toISOString().slice(0, 10) : new Date().toISOString().slice(0, 10),
        Validators.required
      ],
      purpose: [e?.purpose ?? ''],
      tags: [(e?.tagNames ?? []).join(', ')]
    });
  }

  ngAfterViewInit(): void {
    if (this.isEdit) return;

    setTimeout(() => this.host.nativeElement.querySelector<HTMLInputElement>('#amount')?.focus());
  }

  private parseTags(): string[] {
    const raw = String(this.form.value.tags ?? '');

    return [
      ...new Set(
        raw
          .split(',')
          .map(tag => tag.trim())
          .filter(Boolean)
      )
    ];
  }

  get isExpense(): boolean {
    return this.type === 'expense';
  }

  toggleNewCategory(): void {
    this.showNewCategory = !this.showNewCategory;
    this.newCategoryName = '';
  }

  createCategory(): void {
    if (!this.newCategoryName.trim()) return;
    const catType = this.isExpense ? CategoryType.Expenses : CategoryType.Incomes;
    this.categoryService
      .create({
        name: this.newCategoryName.trim(),
        type: catType,
        plannedPeriodAmount: 0,
        period: PerPeriodType.None
      })
      .subscribe({
        next: id => {
          const newCat: CategoryResponse = {
            id,
            name: this.newCategoryName.trim(),
            type: catType,
            periodAmount: 0,
            periodType: PerPeriodType.None,
            isDeleted: false
          };
          this.categories = [...this.categories, newCat];
          this.form.patchValue({ categoryId: id });
          this.showNewCategory = false;
          this.newCategoryName = '';
          this.cdr.markForCheck();
        }
      });
  }

  submit(): void {
    if (this.form.invalid) return;
    const v = this.form.value;
    if (this.isEdit) {
      this.dialogRef.close({
        _edit: true,
        id: this.editId,
        categoryId: Number(v.categoryId),
        amount: Number(v.amount),
        paymentDate: v.paymentDate,
        purpose: v.purpose || null,
        date: v.paymentDate,
        tagNames: this.tagsChanged() ? this.parseTags() : undefined
      });
    } else {
      this.dialogRef.close({
        capitalId: Number(v.capitalId),
        categoryId: Number(v.categoryId),
        amount: Number(v.amount),
        paymentDate: v.paymentDate,
        purpose: v.purpose || null,
        tagNames: this.isExpense && this.parseTags().length ? this.parseTags() : null
      });
    }
  }

  private tagsChanged(): boolean {
    const before = (this.data.editItem?.tagNames ?? []).join('|');

    return this.isExpense && this.parseTags().join('|') !== before;
  }

  cancel(): void {
    this.dialogRef.close(null);
  }
}
