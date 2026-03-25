import {HttpErrorResponse, HttpInterceptorFn} from '@angular/common/http';
import {inject} from '@angular/core';
import {catchError, throwError} from 'rxjs';
import {Result} from '../models/result-model';
import {ErrorService} from '../../shared/services/error.service';
import {PopupMessageService} from '../../shared/services/popup-message.service';
import {AppError} from '../models/error-model';
import {AuthService} from '../../modules/auth/services/auth-service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const errorService = inject(ErrorService);
  const popupMessageService = inject(PopupMessageService);
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((response: HttpErrorResponse) => {
      const result: Result | null = response.error;

      if (response.status === 401) {
        authService.invalidate();
        return throwError(() => response);
      }

      const appError: AppError = {
        status: response.status,
        message: 'An unexpected error occurred.',
        originalError: response
      };

      if (response.status >= 400 && response.status < 500 && result?.errors?.length) {
        appError.message = 'Validation error';
        appError.errors = result.errors.map(e => e.message);
        popupMessageService.error(appError.errors.join('<br>'));
      }
      else if (response.status === 404) {
        appError.message = 'Resource not found';
        appError.redirectTo = '/error';
        popupMessageService.error('Resource not found');
      }
      else if (response.status >= 500) {
        popupMessageService.error('An unexpected server error occurred');
      }

      errorService.emit(appError);

      return throwError(() => response);
    })
  );
};
