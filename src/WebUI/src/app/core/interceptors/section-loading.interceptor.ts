import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { SectionLoadingService } from '../../shared/services/section-loading.service';

export const sectionLoadingInterceptor: HttpInterceptorFn = (req, next) => {
  const sectionLoading = inject(SectionLoadingService);
  const key = sectionLoading.resolve(req.url);

  if (!key) return next(req);

  sectionLoading.start(key);

  return next(req).pipe(
    finalize(() => sectionLoading.stop(key))
  );
};
