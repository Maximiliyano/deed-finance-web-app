import { Component, Inject } from '@angular/core';
import { DIALOG_DATA } from '../models/dialog-consts';
import { DialogRef } from '../models/dialog-ref';

@Component({
    selector: 'app-confirm-dialog',
    templateUrl: './confirm-dialog.component.html',
    styleUrl: './confirm-dialog.component.scss',
    standalone: true
})
export class ConfirmDialogComponent {
  constructor(
    @Inject(DIALOG_DATA) public data: {
      title: string;
      action: string;
    },
    private dialogRef: DialogRef<boolean>
  ) {}

  handleConfirm() {
    this.dialogRef.close(true);
  }

  handleCancel() {
    this.dialogRef.close(false);
  }
}
