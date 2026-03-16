import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
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
  editItem?: { id: number; capitalId: number; categoryId: number; amount: number; paymentDate: string; purpose: string | null };
}

@Component({
  selector: 'app-quick-transaction-dialog',
  templateUrl: './quick-transaction-dialog.component.html',
  styleUrl: './quick-transaction-dialog.component.scss',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule]
})
export class QuickTransactionDialogComponent implements OnInit {
  form!: FormGroup;
  type: 'expense' | 'income';
  capitals: CapitalResponse[] = [];
  categories: CategoryResponse[] = [];
  showNewCategory = false;
  newCategoryName = '';
  isEdit = false;
  editId: number | null = null;

  constructor(
    private readonly fb: FormBuilder,
    private readonly dialogRef: DialogRef<any>,
    private readonly categoryService: CategoryService,
    @Inject(DIALOG_DATA) private readonly data: QuickTransactionDialogData
  ) {
    this.type = data.type;
    this.capitals = data.capitals;
    this.categories = data.categories.filter(c => !c.isDeleted);
  }

  ngOnInit(): void {
    const e = this.data.editItem;
    this.isEdit = !!e;
    this.editId = e?.id ?? null;

    this.form = this.fb.group({
      capitalId: [e?.capitalId ?? null, Validators.required],
      categoryId: [e?.categoryId ?? null, Validators.required],
      amount: [e?.amount ?? null, [Validators.required, Validators.min(0.01)]],
      paymentDate: [e?.paymentDate ? new Date(e.paymentDate).toISOString().slice(0, 10) : new Date().toISOString().slice(0, 10), Validators.required],
      purpose: [e?.purpose ?? '']
    });
  }

  get isExpense(): boolean { return this.type === 'expense'; }

  toggleNewCategory(): void {
    this.showNewCategory = !this.showNewCategory;
    this.newCategoryName = '';
  }

  createCategory(): void {
    if (!this.newCategoryName.trim()) return;
    const catType = this.isExpense ? CategoryType.Expenses : CategoryType.Incomes;
    this.categoryService.create({
      name: this.newCategoryName.trim(),
      type: catType,
      plannedPeriodAmount: 0,
      period: PerPeriodType.None
    }).subscribe({
      next: (id) => {
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
        date: v.paymentDate
      });
    } else {
      this.dialogRef.close({
        capitalId: Number(v.capitalId),
        categoryId: Number(v.categoryId),
        amount: Number(v.amount),
        paymentDate: v.paymentDate,
        purpose: v.purpose || null,
        tagNames: null
      });
    }
  }

  cancel(): void {
    this.dialogRef.close(null);
  }
}
