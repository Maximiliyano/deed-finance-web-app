import {HttpInterceptorFn} from '@angular/common/http';
import {retry, timer} from 'rxjs';

export const retryInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.method !== 'GET') return next(req);

  return next(req).pipe(
    retry({
      count: 2,
      delay: (error, retryCount) => {
        if (error.status === 0 || error.status >= 500) {
          return timer(retryCount * 1000);
        }
        throw error;
      }
    })
  );
};
