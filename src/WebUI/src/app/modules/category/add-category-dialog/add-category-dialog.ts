import { Component, Inject, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { CategoryType } from '../../../core/types/category-type';
import { PerPeriodType } from '../../../core/types/per-period-type';
import { enumToOptions } from '../../../core/utils/enum';
import { DIALOG_DATA } from '../../../shared/components/dialogs/models/dialog-consts';
import { DialogRef } from '../../../shared/components/dialogs/models/dialog-ref';
import { FormField } from '../../../shared/components/forms/models/form-field';
import { SharedModule } from '../../../shared/shared.module';
import { CategoryResponse } from '../models/category-model';
import { FormComponent } from '../../../shared/components/forms/form.component';

@Component({
  selector: 'app-add-category-dialog',
  templateUrl: './add-category-dialog.html',
  styleUrl: './add-category-dialog.scss',
  standalone: true,
  imports: [SharedModule, FormComponent]
})
export class AddCategoryDialog implements OnInit {
  form: FormGroup;
  fields: FormField[] = [];

  private periodTypeOptions = enumToOptions(PerPeriodType);
  private categoryTypeOptions = enumToOptions(CategoryType, {
    exclude: ['None', ...this.exclude]
  });

  constructor(
    @Inject(DIALOG_DATA) public exclude: string[],
    private readonly dialogRef: DialogRef<CategoryResponse | null>
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.initFields();
  }

  private initForm(): void {
    this.form = new FormGroup({
      Name: new FormControl('', [Validators.required, Validators.minLength(1), Validators.maxLength(32)]),
      Type: new FormControl({ value: CategoryType.Expenses, disabled: this.categoryTypeOptions.length < 2 }, [Validators.required]),
      PeriodAmount: new FormControl(0, [Validators.min(0)]),
      PeriodType: new FormControl(PerPeriodType.None)
    });
  }

  private initFields(): void {
    this.fields = [
      {
        label: 'Name',
        controlName: 'Name',
        input: {
          type: 'text',
          placeholder: 'Please enter category name...'
        }
      },
      {
        label: 'Type',
        controlName: 'Type',
        select: {
          options: this.categoryTypeOptions
        }
      },
      {
        label: 'Planned amount',
        controlName: 'PeriodAmount',
        input: {
          type: 'number',
          placeholder: 'Please type period amount...'
        }
      },
      {
        label: 'Period of amount',
        controlName: 'PeriodType',
        select: {
          options: this.periodTypeOptions
        }
      }
    ];
  }

  handleSubmit(): void {
    if (this.form.invalid) {
      return;
    }

    const updated = this.form.getRawValue();

    this.dialogRef.close({
      id: -1,
      name: updated.Name,
      type: updated.Type,
      periodAmount: updated.PeriodAmount,
      periodType: updated.PeriodType,
      isDeleted: false
    });
  }

  handleCancel(): void {
    this.dialogRef.close();
  }
}
