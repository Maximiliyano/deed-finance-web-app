import { Component, Inject, OnDestroy } from '@angular/core';
import { CategoryResponse } from '../../../../core/models/category-model';
import { Subject, takeUntil } from 'rxjs';
import { PerPeriodType } from '../../../../core/types/per-period-type';
import { enumToOptions } from '../../../../core/utils/enum';
import { DIALOG_DATA } from '../../dialogs/models/dialog-consts';
import { DialogRef } from '../../dialogs/models/dialog-ref';
import { SharedModule } from "../../../shared.module";
import { DialogService } from '../../dialogs/services/dialog.service';
import { AddCategoryDialog } from '../add-category-dialog/add-category-dialog';
import { PopupMessageService } from '../../../services/popup-message.service';
import { CategoryService } from '../../../services/category.service';
import { CategoryType } from '../../../../core/types/category-type';

@Component({
    selector: 'app-categories-dialog-component',
    templateUrl: './categories-dialog-component.html',
    styleUrl: './categories-dialog-component.scss',
    standalone: true,
    imports: [SharedModule]
})
export class CategoriesDialogComponent implements OnDestroy {
  periodTypeOptions = enumToOptions(PerPeriodType);
  PerPeriodType = PerPeriodType;
  
  editableCategories: CategoryResponse[] = [];

  changed = new Set<number>();
  deleted = new Set<number>();
  
  isEditModeEnabled: boolean = false;

  private $unsubscribe = new Subject<void>();
  
  constructor(
    @Inject(DIALOG_DATA) public data: { originalCategories: CategoryResponse[], currency: string, type?: string },
    private dialogRef: DialogRef<CategoryResponse[]>,
    private readonly dialogService: DialogService,
    private readonly popupMessageService: PopupMessageService,
    private readonly categoryService: CategoryService
  ) {
    this.editableCategories = this.data.originalCategories.map(c => ({ ...c }));
  }

  ngOnDestroy(): void {
    this.$unsubscribe.next();
    this.$unsubscribe.complete();
  }

  get hasChanges(): boolean {
    return this.changed.size > 0 || this.deleted.size > 0;
  }

  add(): void {
    if (this.isEditModeEnabled) return;

    const ref = this.dialogService.open(AddCategoryDialog, {
      data: [CategoryType[CategoryType.Incomes]]
    });
    
    ref.afterClosed$.subscribe({
      next: (response: CategoryResponse | null) => {
        if (!response) return;

        this.categoryService
        .create({
          name: response.name,
            type: Number(response.type),
            plannedPeriodAmount: response.periodAmount,
            period: Number(response.periodType)
          })
          .pipe(takeUntil(this.$unsubscribe))
          .subscribe({
            next: (id: number) => {
              response.id = id;
              
              this.changed.add(response.id);
              this.editableCategories.push(response);
              this.popupMessageService.success(`Category ${response.name} successfully added.`);   
            }
          });
      }
    })
  }

  onFieldChange(id: number): void {
    this.changed.add(id);
    this.editableCategories = [...this.editableCategories];
  }

  save(): void {
    this.dialogRef.close(this.editableCategories);
  }

  remove(id: number): void {
    const founded = this.editableCategories.find(c => c.id === id);
    if (!founded) return;

    this.categoryService
      .delete(id)
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe({
        next: () => {
          this.editableCategories = this.editableCategories.filter(x => x.id !== id);
          this.isEditModeEnabled = this.editableCategories.length > 0 && this.isEditModeEnabled;
          this.popupMessageService.success(`Category ${founded.name} removed`);
        }
      });
  }

  cancel(): void {
    this.isEditModeEnabled = false;

    this.changed.clear();
    this.deleted.clear();

    this.editableCategories = this.data.originalCategories.map(c => ({...c}));
  }
}
