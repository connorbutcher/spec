import { Injectable } from '@angular/core';

export type BrowserSendRequest = {
  targetWindow: Window;
  targetOrigin: string;
  message: unknown;
  retryIntervalMs?: number;
  maxAttempts?: number;
};

export type BrowserResponseRequest<T> = BrowserSendRequest & {
  responseGuard: (data: unknown) => data is T;
  timeoutMs?: number;
};

export type BrowserReceiveRequest<T> = {
  sourceOrigin: string;
  sourceWindow?: Window;
  messageGuard: (data: unknown) => data is T;
};

@Injectable({
  providedIn: 'root',
})
export class BrowserCommunicationService {
  private static readonly DEFAULT_TIMEOUT_MS = 30000;

  public send(request: BrowserSendRequest): () => void {
    const retryIntervalMs = request.retryIntervalMs ?? 0;
    const maxAttempts = Math.max(1, request.maxAttempts ?? 1);

    let attempts = 0;
    let intervalId: number | null = null;

    const stop = (): void => {
      if (!intervalId) {
        return;
      }

      window.clearInterval(intervalId);
      intervalId = null;
    };

    const post = (): void => {
      if (request.targetWindow.closed) {
        stop();
        return;
      }

      request.targetWindow.postMessage(request.message, request.targetOrigin);
      attempts += 1;

      if (attempts >= maxAttempts) {
        stop();
      }
    };

    post();

    if (retryIntervalMs > 0 && maxAttempts > 1 && !request.targetWindow.closed) {
      intervalId = window.setInterval(post, retryIntervalMs);
    }

    return stop;
  }

  public requestResponse<T>(request: BrowserResponseRequest<T>): Promise<T> {
    const timeoutMs = Math.max(1, request.timeoutMs ?? BrowserCommunicationService.DEFAULT_TIMEOUT_MS);

    return new Promise<T>((resolve, reject) => {
      let finished = false;
      const cancelSend = this.send(request);

      const complete = (): void => {
        if (finished) {
          return;
        }

        finished = true;
        cancelSend();
        window.removeEventListener('message', onMessage);
        clearTimeout(timeoutId);
      };

      const onMessage = (event: MessageEvent<unknown>): void => {
        if (event.source !== request.targetWindow) {
          return;
        }

        if (!this.matchesOrigin(request.targetOrigin, event.origin)) {
          return;
        }

        if (!request.responseGuard(event.data)) {
          return;
        }

        complete();
        resolve(event.data);
      };

      const timeoutId = window.setTimeout(() => {
        complete();
        reject(new Error(`Timed out waiting for message response from origin "${request.targetOrigin}".`));
      }, timeoutMs);

      window.addEventListener('message', onMessage);
    });
  }

  public listen<T>(request: BrowserReceiveRequest<T>, handler: (message: T, event: MessageEvent<T>) => void): () => void {
    const onMessage = (event: MessageEvent<unknown>): void => {
      if (request.sourceWindow && event.source !== request.sourceWindow) {
        return;
      }

      if (event.origin !== request.sourceOrigin) {
        return;
      }

      if (!request.messageGuard(event.data)) {
        return;
      }

      handler(event.data, event as MessageEvent<T>);
    };

    window.addEventListener('message', onMessage);

    return () => {
      window.removeEventListener('message', onMessage);
    };
  }

  public reply(event: MessageEvent<unknown>, message: unknown, targetOrigin = event.origin): void {
    const source = event.source;
    if (!this.isWindowMessageSource(source)) {
      return;
    }

    source.postMessage(message, targetOrigin);
  }

  public registerResponder<TRequest, TResponse>(
    request: BrowserReceiveRequest<TRequest>,
    createResponse: (message: TRequest, event: MessageEvent<TRequest>) => TResponse | undefined,
  ): () => void {
    return this.listen(request, (message, event) => {
      const response = createResponse(message, event);

      if (response === undefined) {
        return;
      }

      this.reply(event, response);
    });
  }

  public resolveOrigin(url: string): string | null {
    try {
      return new URL(url, window.location.origin).origin;
    } catch {
      return null;
    }
  }

  private matchesOrigin(expectedOrigin: string, actualOrigin: string): boolean {
    return expectedOrigin === '*' || actualOrigin === expectedOrigin;
  }

  private isWindowMessageSource(source: MessageEventSource | null): source is Window {
    return (
      source !== null &&
      typeof source === 'object' &&
      'closed' in source &&
      typeof source.closed === 'boolean' &&
      'postMessage' in source &&
      typeof source.postMessage === 'function'
    );
  }
}
