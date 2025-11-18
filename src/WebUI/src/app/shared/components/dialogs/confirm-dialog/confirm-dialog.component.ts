import { Component, HostListener, Inject } from '@angular/core';
import { DIALOG_DATA } from '../models/dialog-consts';
import { DialogRef } from '../models/dialog-ref';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-confirm-dialog',
    templateUrl: './confirm-dialog.component.html',
    styleUrl: './confirm-dialog.component.scss',
    imports: [CommonModule],
    standalone: true
})
export class ConfirmDialogComponent {
  title: string;
  message: string;
  submitText: string;
  cancelText: string;
  icon: 'warning' | 'danger' | 'info' | 'none';

  constructor(
    @Inject(DIALOG_DATA) public data: {
      title?: string;
      message?: string;
      submitText?: string;
      cancelText?: string;
      icon?: 'warning' | 'danger' | 'info' | 'none'
    },
    private dialogRef: DialogRef<boolean>
  ) {
    this.title = data.title ?? 'Confirm';
    this.message = data.message ?? 'Are you sure you want to continue?';
    this.submitText = data.submitText ?? 'Confirm';
    this.cancelText = data.cancelText ?? 'Cancel';
    this.icon = data.icon ?? 'warning';
  }

  handleConfirm() {
    this.dialogRef.close(true);
  }

  handleCancel() {
    this.dialogRef.close(false);
  }

  @HostListener('document:keydown.enter')
  handleEnter() {
    this.handleConfirm();
  }

  @HostListener('document:keydown.escape')
  handleEscape() {
    this.handleCancel();
  }
}
