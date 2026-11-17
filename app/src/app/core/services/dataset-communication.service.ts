import { Injectable, inject } from '@angular/core';
import { PUCardAnalyticsService } from '../../features/analytics/services/pu-card-analytics.service';
import { Dataset } from '../models/analytics/dataset';
import { CompleteMessage } from '../models/browser-communication/complete-message';
import { DatasetMessage } from '../models/browser-communication/dataset-message';
import { BrowserCommunicationService } from './browser-communication.service';

@Injectable({
  providedIn: 'root',
})
export class DatasetCommunicationService {
  private readonly _browserCommunicationService = inject(
    BrowserCommunicationService,
  );
  private readonly _puCardAnalyticsService = inject(PUCardAnalyticsService);
  private _completeSent = false;

  public async sendDatasetUpdateToParent(dataset: Dataset): Promise<void> {
    if (!window.opener) {
      return Promise.reject(new Error('No parent window available'));
    }

    await this._browserCommunicationService.requestResponse({
      targetWindow: window.opener,
      targetOrigin: '*',
      message: {
        type: 'dataset',
        status: 'pending',
        payload: dataset,
      },
      responseGuard: (data: unknown): data is DatasetMessage => {
        if (!data || typeof data !== 'object') {
          return false;
        }

        const msg = data as Record<string, unknown>;

        return (
          msg['type'] === 'dataset' &&
          msg['status'] === 'success' &&
          typeof msg['payload'] === 'object' &&
          msg['payload'] !== null
        );
      },
    });
  }

  public sendCompleteToParent(): void {
    if (
      this._completeSent ||
      !this._puCardAnalyticsService.isEnabled() ||
      !window.opener
    ) {
      return;
    }

    this._completeSent = true;

    this._browserCommunicationService.send({
      targetWindow: window.opener,
      targetOrigin: '*',
      message: {
        type: 'complete',
        status: 'pending',
        payload: null,
      } satisfies CompleteMessage,
    });
  }
}
