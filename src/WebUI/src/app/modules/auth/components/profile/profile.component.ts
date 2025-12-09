import { Component, OnDestroy, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth-service';
import { Subject, takeUntil } from 'rxjs';
import { SharedModule } from "../../../../shared/shared.module";
import { DialogService } from '../../../../shared/components/dialogs/services/dialog.service';
import { ConfirmDialogComponent } from '../../../../shared/components/dialogs/confirm-dialog/confirm-dialog.component';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';
import { User } from '../../models/user';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  standalone: true,
  imports: [SharedModule]
})
export class ProfileComponent implements OnInit, OnDestroy {
  user: User | null;
  editMode: boolean;

  private $unsubscribe = new Subject<void>();

  constructor(
    private readonly authService: AuthService,
    private readonly dialogService: DialogService,
    private readonly popupMessageService: PopupMessageService
  ) {}

  ngOnInit(): void {
    this.authService.user$
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe(user => {
        this.user = user;
        document.title = `Deed - ${this.user?.fullname ?? ''} profile`
      });
  }

  ngOnDestroy(): void {
    this.$unsubscribe.next();
    this.$unsubscribe.complete();
  }

  handleEdit(): void {
    if (this.editMode) { // TODO save, sends new user

    }

    this.editMode = !this.editMode;
  }

  handleLogout(): void {
    const dialogRef = this.dialogService.open(ConfirmDialogComponent, {
      data: {
        title: 'Log out',
        message: 'Are you sure you want to log out your account?',
        submitText: 'Yes',
        cancelText: 'Cancel',
        icon: 'warning'
      }
    });

    dialogRef
      .afterClosed$
      .pipe(takeUntil(this.$unsubscribe))
      .subscribe({
        next: (submit: boolean) => {
          if (submit) {
            this.authService.logout();
            this.popupMessageService.warning('You logged out your account.');
          }
        }
      });
  }
}
