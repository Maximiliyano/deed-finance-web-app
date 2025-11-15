import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { Result } from '../models/result-model';
import { Router } from '@angular/router';
import { ErrorService } from '../../shared/services/error.service';
import { PopupMessageService } from '../../shared/services/popup-message.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const errorService = inject(ErrorService);
  const popupMessageService = inject(PopupMessageService);
  // TODO there exist error 0, when try to updateRange categories
  // TODO auto redirect on previous url when error solved

  return next(req).pipe(
    catchError((response: HttpErrorResponse) => {
      const previousUrl = router.url;
      const result: Result | null = response.error;

      if (response.status === 401) {
        popupMessageService.warning('You need to authenticate before accessing this page');
        router.navigate([previousUrl]);
      }
      else if (response.status >= 400 && response.status < 500 && result) {
        result.errors.map(e => {
          popupMessageService.error(e.message);
        });
      }
      else {
        errorService.setError({ status: response.status, message: 'An unexcepted error occured.' }, previousUrl);
      }
      
      if (router.url !== '/error' && response.status === 404) {
        router.navigate(['/error']);
      }

      return throwError(() => response);
    })
  );
};
