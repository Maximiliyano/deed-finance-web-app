import { Component } from '@angular/core';
import { CategoryResponse } from '../../../../core/models/category-model';

@Component({
  selector: 'app-categories-dialog-component',
  templateUrl: './categories-dialog-component.html',
  styleUrl: './categories-dialog-component.scss'
})
export class CategoriesDialogComponent {
  categories: CategoryResponse[] = [];
}
