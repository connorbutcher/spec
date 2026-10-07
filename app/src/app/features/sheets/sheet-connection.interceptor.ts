import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { SheetConnectionId } from './sheet-connection-id';

/** Names this tab's live connection on every change it sends to the API. */
export const sheetConnectionInterceptor: HttpInterceptorFn = (request, next) => {
  const connectionId = inject(SheetConnectionId).value();
  if (connectionId === null || request.method === 'GET' || !request.url.startsWith('/api/')) {
    return next(request);
  }
  return next(request.clone({ setHeaders: { 'X-Sheet-Connection': connectionId } }));
};
