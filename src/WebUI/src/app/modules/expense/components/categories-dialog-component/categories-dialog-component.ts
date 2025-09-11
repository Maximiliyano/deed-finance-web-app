import { Component, OnDestroy, OnInit } from '@angular/core';
import { CategoryResponse } from '../../../../core/models/category-model';
import { CategoryApiService } from '../../services/category.api.service';
import { DialogService } from '../../../../shared/services/dialog.service';
import { ConfirmDialogComponent } from '../../../../shared/components/dialogs/confirm-dialog/confirm-dialog.component';
import { Subject, takeUntil } from 'rxjs';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';

@Component({
  selector: 'app-categories-dialog-component',
  templateUrl: './categories-dialog-component.html',
  styleUrl: './categories-dialog-component.scss'
})
export class CategoriesDialogComponent implements OnInit, OnDestroy {
  categories: CategoryResponse[] = [];

  isEditModeEnabled: boolean = false;

  private $unsubscribe = new Subject<void>;

  constructor(
    private readonly popupMessageService: PopupMessageService,
    private readonly dialogService: DialogService,
    private readonly categoryApiService: CategoryApiService) {}

  ngOnInit(): void {
  }

  ngOnDestroy(): void {
    this.$unsubscribe.next();
    this.$unsubscribe.complete();
  }

  addNewCategory(): void {

  }

  editAllCategories(): void {
    this.isEditModeEnabled = !this.isEditModeEnabled;
  }

  removeCategory(id: number): void {
    this.dialogService.open({
      component: ConfirmDialogComponent,
      data: {
        title: 'category',
        action: 'delete'
      },
      onSubmit: (result: boolean) => {
        if (result) {
          this.categoryApiService
            .delete(id)
            .pipe(takeUntil(this.$unsubscribe))
            .subscribe({
              next: () => this.removeCategoryFromList(id)
            });
        }
        else {
          this.dialogService.close();
        }
      }
    })
  }

  removeCategoryFromList(id: number): void {
    this.categories = this.categories.filter(c => c.id !== id);
    this.popupMessageService.success('Category removed');
    this.dialogService.close();
  }
}
