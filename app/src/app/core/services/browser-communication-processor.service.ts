import { inject, Injectable } from '@angular/core';
import { PUCardAnalyticsService } from '../../features/analytics/services/pu-card-analytics.service';
import { Dataset } from '../models/analytics/dataset';
import { BrowserMessage } from '../models/browser-communication/browser-message';
import { BrowserCommunicationService } from './browser-communication.service';
import { WindowMessageListenerService } from './window-message-listener.service';

@Injectable({
  providedIn: 'root',
})
export class BrowserCommunicationProcessorService {
  private readonly _browserCommunicationService = inject(
    BrowserCommunicationService,
  );
  private readonly _puCardAnalyticsService = inject(PUCardAnalyticsService);
  private readonly _windowMessageListenerService = inject(
    WindowMessageListenerService,
  );

  constructor() {
    this._windowMessageListenerService.registerHandler((message) =>
      this.process(message),
    );
  }

  public process<T extends MessageEvent<BrowserMessage<unknown>>>(
    message: T,
  ): void {
    switch (message.data.type) {
      case 'initialize': {
        this._browserCommunicationService.reply(message, {
          status: 'success',
          type: message.data.type,
          payload: message.data.payload,
          message: 'Message received and processed successfully',
        });
        this._puCardAnalyticsService.isEnabled.set(true);
        break;
      }
      case 'dataset': {
        const payload = message.data.payload as Dataset;
        if (message.data.status === 'success') {
          return;
        }

        this._browserCommunicationService.reply(message, {
          status: 'success',
          type: message.data.type,
          payload: message.data.payload,
          message: 'Message received and processed successfully',
        });
        this._puCardAnalyticsService.currentDataset.set(payload);
        break;
      }
    }
  }
}
