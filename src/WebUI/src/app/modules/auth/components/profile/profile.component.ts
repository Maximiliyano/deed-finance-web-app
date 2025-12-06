import { Component, OnDestroy, OnInit } from '@angular/core';
import { AuthService, User } from '../../services/auth-service';
import { Subject, takeUntil } from 'rxjs';
import { Router } from '@angular/router';
import { PopupMessageService } from '../../../../shared/services/popup-message.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  standalone: true
})
export class ProfileComponent implements OnInit, OnDestroy {
  user: User;

  private unsubscribe$ = new Subject<void>();

  constructor(
    private readonly popupMessageService: PopupMessageService,
    private readonly authService: AuthService,
    private readonly router: Router) {}

  ngOnInit(): void {
    this.authService.me()
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe({
        next: (user) => {
          if (user) {
            this.user = user
          }
          else {
            this.router.navigate(['/']);
            this.popupMessageService.error('Unexcepted error occured. User is not found.')
          }
        }
      })
  }
  ngOnDestroy(): void {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }
}
