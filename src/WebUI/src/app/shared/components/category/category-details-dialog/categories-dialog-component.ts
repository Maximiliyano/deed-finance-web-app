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
import { ConfirmDialogComponent } from '../../dialogs/confirm-dialog/confirm-dialog.component';

@Component({
    selector: 'app-categories-dialog-component',
    templateUrl: './categories-dialog-component.html',
    styleUrl: './categories-dialog-component.scss',
    standalone: true,
    imports: [SharedModule]
})
export class CategoriesDialogComponent implements OnDestroy {
  periodTypeOptions = enumToOptions(PerPeriodType);
  
  isEditModeEnabled: boolean = false;
  
  private $unsubscribe = new Subject<void>();

  PerPeriodType = PerPeriodType;

  initCategories: CategoryResponse[];
  
  constructor(
    @Inject(DIALOG_DATA) public data: { categories: CategoryResponse[], currency: string, type?: string },
    private dialogRef: DialogRef<CategoryResponse[]>,
    private readonly dialogService: DialogService,
    private readonly popupMessageService: PopupMessageService,
    private readonly categoryService: CategoryService
  ) {
    this.initCategories = [...this.data.categories];
  }

  ngOnDestroy(): void {
    this.$unsubscribe.next();
    this.$unsubscribe.complete();
  }

  hasChanges(): boolean {
    return this.data.categories.some((cat, index) => {
      const original = this.initCategories[index];
      return !this.isCategoryEqual(cat, original);
    });
  }

  private isCategoryEqual(a: CategoryResponse, b: CategoryResponse): boolean {
    return a.id === b.id &&
          a.name === b.name &&
          a.type === b.type &&
          a.periodType === b.periodType &&
          a.periodAmount === b.periodAmount;
  }

  addCategory(): void {
    const ref = this.dialogService.open(AddCategoryDialog);
    
    ref
      .afterClosed$
      .subscribe({
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
                
                this.data.categories.push(response);
                this.popupMessageService.success(`Category ${response.name} successfully added.`);   
              }
            });
        }
      })
  }

  editCategories(): void {
    // TODO disable buttons if there is not any update
    if (!this.hasChanges()) {
      return;
    }
    this.dialogRef.close(this.data.categories);
  }

  deleteCategory(id: number): void {
    const ref = this.dialogService.open(ConfirmDialogComponent, {
      data: {
        title: 'category',
        action: 'delete'
      }
    });

    ref
      .afterClosed$
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe({
        next: (result: boolean) => {
          if (result) {
            const founded = this.data.categories.find(c => c.id === id);
            if (!founded) return;
        
            this.categoryService
              .delete(id)
              .pipe(takeUntil(this.$unsubscribe))
              .subscribe({
                next: () => {
                  this.data.categories = this.data.categories.filter(x => x.id !== id);
                  this.popupMessageService.success(`Category ${founded.name} removed`);
                }
              });
          }
        }
      });
  }
}
