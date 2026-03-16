import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { map, catchError, of, tap } from 'rxjs';
import { AuthService } from '../../modules/auth/services/auth-service';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);

  return authService.me().pipe(
    tap(user => {
      if (!user) authService.login();
    }),
    map(user => !!user),
    catchError(() => of(false))
  );
};
