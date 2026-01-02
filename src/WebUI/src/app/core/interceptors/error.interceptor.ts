import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { Result } from '../models/result-model';
import { Router } from '@angular/router';
import { ErrorService } from '../../shared/services/error.service';
import { PopupMessageService } from '../../shared/services/popup-message.service';
import { AppError } from '../models/error-model';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const errorService = inject(ErrorService);
  const popupMessageService = inject(PopupMessageService);
  // TODO auto redirect on previous url when error solved

  return next(req).pipe(
    catchError((response: HttpErrorResponse) => {
      const result: Result | null = response.error;

      const appError: AppError = {
        status: response.status,
        message: 'An unexcepted error occurred.',
        originalError: response
      };

      if (response.status === 401) {
        appError.message = 'You are not authenticated.';
        appError.redirectTo = '/login';
      }
      else if (response.status >= 400 && response.status < 500 && result) {
        appError.message = 'Validation error';
        appError.errors = result.errors.map(e => e.message);
      }
      else if (response.status === 404) {
        appError.message = 'Resource not found';
        appError.redirectTo = '/error';
      }
      
      errorService.emit(appError);

      return throwError(() => response);
    })
  );
};
