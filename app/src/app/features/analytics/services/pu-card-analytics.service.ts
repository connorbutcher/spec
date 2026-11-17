import { Injectable, signal } from '@angular/core';
import { Dataset } from '../../../core/models/analytics/dataset';

@Injectable({
  providedIn: 'root',
})
export class PUCardAnalyticsService {
  public readonly isEnabled = signal<boolean>(false);
  public readonly currentDataset = signal<Dataset | null>(null);
}
