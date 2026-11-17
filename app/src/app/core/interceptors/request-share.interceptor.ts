import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { Observable, ReplaySubject, finalize } from 'rxjs';

export class RequestShareInterceptor implements HttpInterceptor {
  private requests: Map<string, ReplaySubject<HttpEvent<unknown>>>;

  constructor() {
    this.requests = new Map();
  }

  public intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler,
  ): Observable<HttpEvent<unknown>> {
    if (request.method !== 'GET') {
      return next.handle(request);
    }

    const cacheKey: string = request.urlWithParams;

    if (this.requests.has(cacheKey)) {
      const request = this.requests.get(cacheKey);
      if (request) {
        return request.asObservable();
      }
    }

    const subject: ReplaySubject<HttpEvent<unknown>> = new ReplaySubject<
      HttpEvent<unknown>
    >(1);

    this.requests.set(cacheKey, subject);

    const deleteFromRequests = (): void => {
      this.requests.delete(cacheKey);
    };

    next.handle(request).pipe(finalize(deleteFromRequests)).subscribe(subject);

    return subject.asObservable();
  }
}
