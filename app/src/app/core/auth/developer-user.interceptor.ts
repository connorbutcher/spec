import { HttpInterceptorFn } from '@angular/common/http';
import { DEVELOPER_USER } from './developer-user';

/** Development only: says which user this tab runs as on every call to the API. */
export const developerUserInterceptor: HttpInterceptorFn = (request, next) => {
  if (DEVELOPER_USER === null || !request.url.startsWith('/api/')) {
    return next(request);
  }
  return next(request.clone({ setHeaders: { 'X-Developer-User': DEVELOPER_USER } }));
};
