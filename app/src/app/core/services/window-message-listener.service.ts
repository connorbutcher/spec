import { Injectable, OnDestroy, inject } from '@angular/core';
import { BrowserMessage } from '../models/browser-communication/browser-message';
import { UserService } from './user.service';

export type MessageHandler = (
  event: MessageEvent<BrowserMessage<unknown>>,
) => void;

@Injectable({
  providedIn: 'root',
})
export class WindowMessageListenerService implements OnDestroy {
  private readonly _userService = inject(UserService);
  private _handlers: MessageHandler[] = [];
  private _isListening = false;

  public registerHandler(handler: MessageHandler): void {
    this._handlers.push(handler);
    this.setupListener();
  }

  public unregisterHandler(handler: MessageHandler): void {
    this._handlers = this._handlers.filter((h) => h !== handler);
  }

  private setupListener(): void {
    if (this._isListening) {
      return;
    }

    this._isListening = true;
    window.addEventListener('message', (event) => this.onWindowMessage(event));
  }

  private onWindowMessage(event: MessageEvent<unknown>): void {
    if (!this.isBrowserMessage(event.data) || !this.isLoggedIn()) {
      return;
    }

    const resolvedEvent = event as MessageEvent<BrowserMessage<unknown>>;

    for (const handler of this._handlers) {
      handler(resolvedEvent);
    }
  }

  private isBrowserMessage(data: unknown): data is BrowserMessage<unknown> {
    return (
      !!data && typeof data === 'object' && 'type' in data && 'status' in data
    );
  }

  private isLoggedIn(): boolean {
    return !!this._userService.user();
  }

  ngOnDestroy(): void {
    this._handlers = [];
    this._isListening = false;
  }
}
