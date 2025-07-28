import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../components/dialogs/confirm-dialog/confirm-dialog.component';
import { lastValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ConfirmDialogService {
  constructor(private readonly dialog: MatDialog) { }

  toggle(title: string, action: string): Promise<boolean> {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {title, action},
    });

    return lastValueFrom(dialogRef.afterClosed());
  }
}
