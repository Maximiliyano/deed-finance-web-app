import {inject} from '@angular/core';
import {CanActivateFn} from '@angular/router';
import {catchError, map, of} from 'rxjs';
import {AuthService} from '../../modules/auth/services/auth-service';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);

  return authService.me().pipe(
    map(user => {
      if (!user) {
        authService.login();
        return false;
      }
      return true;
    }),
    catchError(() => {
      authService.login();
      return of(false);
    })
  );
};
