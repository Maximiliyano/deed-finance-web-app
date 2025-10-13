import { Component, EventEmitter, OnDestroy, Output } from '@angular/core';
import { CategoryResponse } from '../../../../core/models/category-model';
import { Subject } from 'rxjs';
import { DialogState } from '../../../../shared/components/dialogs/dialog.state';

@Component({
  selector: 'app-categories-dialog-component',
  templateUrl: './categories-dialog-component.html',
  styleUrl: './categories-dialog-component.scss'
})
export class CategoriesDialogComponent implements OnDestroy {
  categories: CategoryResponse[] = [];

  @Output() submitted = new EventEmitter<DialogState>();

  isEditModeEnabled: boolean = false;

  private $unsubscribe = new Subject<void>;

  ngOnDestroy(): void {
    this.$unsubscribe.next();
    this.$unsubscribe.complete();
  }

  addNewCategory(): void {
    this.submitted.emit({
      action: 'create'
    })
  }

  editAllCategories(): void {
    // TODO here is a check if values are changed
    this.submitted.emit({
      data: this.categories,
      action: 'update'
    });
  }

  removeCategory(id: number): void {
    this.submitted.emit({
      data: id,
      action: 'delete'
    });
  }
}
