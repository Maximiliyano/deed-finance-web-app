import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth-service';
import { map } from 'rxjs';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);

  return authService.me().pipe(
    map(user => {
      if (user) 
        return true;
      
      authService.login()
      return false;
    })
  )
};
