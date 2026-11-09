import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogRef } from '../../../../shared/components/dialogs/models/dialog-ref';
import { DIALOG_DATA } from '../../../../shared/components/dialogs/models/dialog-consts';
import { CapitalResponse } from '../../../capital/models/capital-response';
import { CategoryResponse } from '../../../category/models/category-model';
import { CreateUtilityBillRequest, UtilityBillResponse } from '../../models/utility-bill';
import { CURRENCIES } from '../../../../core/types/currency-type';

export interface UtilityBillDialogData {
  bill?: UtilityBillResponse;
  capitals: CapitalResponse[];
  categories: CategoryResponse[];
}

@Component({
  selector: 'app-utility-bill-dialog',
  templateUrl: './utility-bill-dialog.component.html',
  styleUrl: './utility-bill-dialog.component.scss',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UtilityBillDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject<DialogRef<CreateUtilityBillRequest | null>>(DialogRef);
  private readonly data = inject<UtilityBillDialogData>(DIALOG_DATA);

  form!: FormGroup;
  capitals: CapitalResponse[] = [];
  categories: CategoryResponse[] = [];
  currencies = CURRENCIES;
  isEdit = false;

  ngOnInit(): void {
    this.capitals = this.data.capitals ?? [];
    this.categories = this.data.categories ?? [];
    this.isEdit = !!this.data.bill;

    const src = this.data.bill;
    this.form = this.fb.group({
      name: [src?.name ?? '', [Validators.required, Validators.maxLength(64)]],
      estimatedAmount: [src?.estimatedAmount ?? 0, [Validators.required, Validators.min(0)]],
      currency: [src?.currency ?? 'UAH', Validators.required],
      dueDayOfMonth: [src?.dueDayOfMonth ?? 1, [Validators.required, Validators.min(1), Validators.max(31)]],
      capitalId: [src?.capitalId ?? this.capitals[0]?.id ?? '', Validators.required],
      categoryId: [src?.categoryId ?? this.categories[0]?.id ?? '', Validators.required],
      isActive: [src?.isActive ?? true]
    });
  }

  submit(): void {
    if (this.form.invalid) return;
    const val = this.form.value;
    this.dialogRef.close({
      name: val.name.trim(),
      estimatedAmount: val.estimatedAmount,
      currency: val.currency,
      dueDayOfMonth: Number(val.dueDayOfMonth),
      capitalId: Number(val.capitalId),
      categoryId: Number(val.categoryId),
      isActive: !!val.isActive
    });
  }

  cancel(): void {
    this.dialogRef.close(null);
  }
}
