import { ChangeDetectionStrategy, Component, HostListener, inject } from '@angular/core';
import { DIALOG_DATA } from '../models/dialog-consts';
import { DialogRef } from '../models/dialog-ref';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-confirm-dialog',
    templateUrl: './confirm-dialog.component.html',
    styleUrl: './confirm-dialog.component.scss',
    imports: [CommonModule],
    standalone: true,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConfirmDialogComponent {
  readonly data = inject<{ title?: string; message?: string; submitText?: string; cancelText?: string; icon?: 'warning' | 'danger' | 'info' | 'none' }>(DIALOG_DATA);
  private readonly dialogRef = inject<DialogRef<boolean>>(DialogRef);

  title: string = this.data.title ?? 'Confirm';
  message: string = this.data.message ?? 'Are you sure you want to continue?';
  submitText: string = this.data.submitText ?? 'Confirm';
  cancelText: string = this.data.cancelText ?? 'Cancel';
  icon: 'warning' | 'danger' | 'info' | 'none' = this.data.icon ?? 'warning';


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
