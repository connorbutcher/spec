import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { LocalUserService } from '../services/local-user.service';

/** Tells the API which local user to act as, while auth is disabled for local running. */
@Injectable()
export class LocalUserInterceptor implements HttpInterceptor {
  private readonly _localUserService = inject(LocalUserService);

  public intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler,
  ): Observable<HttpEvent<unknown>> {
    if (
      !this._localUserService.isEnabled ||
      !request.url.startsWith(environment.apiUrl)
    ) {
      return next.handle(request);
    }

    return next.handle(
      request.clone({
        setHeaders: {
          [LocalUserService.HeaderName]: String(
            this._localUserService.selected(),
          ),
        },
      }),
    );
  }
}
