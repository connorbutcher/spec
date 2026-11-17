import { Component, HostListener, inject, Signal } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { DatasetCommunicationService } from '../../../../core/services/dataset-communication.service';
import { PUCardAnalyticsService } from '../../services/pu-card-analytics.service';

@Component({
  selector: 'app-pu-card-analytics-banner',
  imports: [ButtonDirective],
  templateUrl: './pu-card-analytics-banner.html',
  styleUrl: './pu-card-analytics-banner.scss',
})
export class PUCardAnalyticsBanner {
  private readonly _datasetCommunicationService = inject(
    DatasetCommunicationService,
  );
  private readonly _puCardAnalyticsService = inject(PUCardAnalyticsService);

  public completeSelection(): void {
    this._datasetCommunicationService.sendCompleteToParent();
    window.close();
  }

  public get isEnabled(): Signal<boolean> {
    return this._puCardAnalyticsService.isEnabled;
  }

  @HostListener('window:beforeunload')
  @HostListener('window:pagehide')
  public completeSelectionOnUnload(): void {
    this._datasetCommunicationService.sendCompleteToParent();
  }
}
