import { Component, Inject, OnDestroy } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { CategoryType } from '../../../core/types/category-type';
import { PerPeriodType } from '../../../core/types/per-period-type';
import { enumToOptions } from '../../../core/utils/enum';
import { DIALOG_DATA } from '../../../shared/components/dialogs/models/dialog-consts';
import { DialogRef } from '../../../shared/components/dialogs/models/dialog-ref';
import { DialogService } from '../../../shared/components/dialogs/services/dialog.service';
import { PopupMessageService } from '../../../shared/services/popup-message.service';
import { SharedModule } from '../../../shared/shared.module';
import { AddCategoryDialog } from '../add-category-dialog/add-category-dialog';
import { CategoryResponse } from '../models/category-model';
import { CategoryService } from '../services/category.service';

@Component({
    selector: 'app-categories-dialog-component',
    templateUrl: './categories-dialog-component.html',
    styleUrl: './categories-dialog-component.scss',
    standalone: true,
    imports: [SharedModule]
})
export class CategoriesDialogComponent implements OnDestroy { // TODO there is a bug, that collections updated here in child component, won't updated in parent (expenses)
  periodTypeOptions = enumToOptions(PerPeriodType);
  PerPeriodType = PerPeriodType;
  
  selectedDeletedId: number | null = null;

  editableCategories: CategoryResponse[] = [];
  deletedCategories: CategoryResponse[] = [];

  changed = new Set<number>();
  
  isEditModeEnabled: boolean = false;

  private $unsubscribe = new Subject<void>();
  
  constructor(
    @Inject(DIALOG_DATA) public data: {
      originalCategories: CategoryResponse[],
      deletedCategories: CategoryResponse[],
      currency: string,
      type?: string
    },
    private dialogRef: DialogRef<CategoryResponse[]>,
    private readonly dialogService: DialogService,
    private readonly popupMessageService: PopupMessageService,
    private readonly categoryService: CategoryService
  ) {
    this.editableCategories = [...this.data.originalCategories];
    this.deletedCategories = [...this.data.deletedCategories];
  }

  ngOnDestroy(): void {
    this.$unsubscribe.next();
    this.$unsubscribe.complete();
  }

  get hasChanges(): boolean {
    return this.changed.size > 0;
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
    const idx = this.editableCategories.findIndex(c => c.id === id);
    if (idx === -1) return;

    const found = this.editableCategories[idx];

    this.categoryService
      .delete(id)
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe({
        next: () => {
          this.editableCategories.splice(idx, 1);
          
          if (this.editableCategories.length === 0) {
            this.isEditModeEnabled = false;
          }

          const alreadyDeleted = this.deletedCategories.some(c => c.id === id);
          if (!alreadyDeleted) {
            this.deletedCategories.push(found);
          }

          this.popupMessageService.success(`Category ${found.name} removed`);
        }
      });
  }

  restore(): void {
    if (!this.selectedDeletedId) return;
  
    this.categoryService.restore(this.selectedDeletedId)
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe({
        next: (restoredCategory) => {
          this.deletedCategories = this.deletedCategories.filter(c => c.id !== restoredCategory.id);

          const exists = this.editableCategories.some(c => c.id === restoredCategory.id);
          if (!exists) {
            this.editableCategories.push(restoredCategory);
          }

          this.selectedDeletedId = null;
          this.popupMessageService.success('Category restored');
        }
    });
  }

  cancel(): void {
    this.isEditModeEnabled = false;

    this.changed.clear();

    this.editableCategories = [...this.data.originalCategories];
  }
}