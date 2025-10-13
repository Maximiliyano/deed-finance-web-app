import { Component, Output } from '@angular/core';
import { Subject } from 'rxjs';

@Component({
    selector: 'app-confirm-dialog',
    templateUrl: './confirm-dialog.component.html',
    styleUrl: './confirm-dialog.component.scss',
    standalone: false
})
export class ConfirmDialogComponent {
  @Output() submitted = new Subject<boolean>();

  title: string;
  action: string;

  handleConfirm() {
    this.submitted.next(true);
  }

  handleCancel() {
    this.submitted.next(false);
  }
}
