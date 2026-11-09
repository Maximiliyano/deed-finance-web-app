import {HttpErrorResponse, HttpInterceptorFn} from '@angular/common/http';
import {inject} from '@angular/core';
import {catchError, throwError} from 'rxjs';
import {Result} from '../models/result-model';
import {PopupMessageService} from '../../shared/services/popup-message.service';
import {AuthService} from '../../modules/auth/services/auth-service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const popupMessageService = inject(PopupMessageService);
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((response: HttpErrorResponse) => {
      const errors = (response.error as Result | null)?.errors?.map(e => e.message) ?? [];

      if (response.status === 401) {
        authService.invalidate();
      } else if (response.status >= 400 && response.status < 500 && errors.length) {
        popupMessageService.error(errors.join('<br>'));
      } else if (response.status === 404) {
        popupMessageService.error('Resource not found');
      } else if (response.status >= 500) {
        popupMessageService.error('An unexpected server error occurred');
      }

      return throwError(() => response);
    })
  );
};
