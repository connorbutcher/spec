import { inject, Injectable } from '@angular/core';
import { SidePanelService } from '../../../../side-panel/side-panel.service';
import {
  ReferencesSidePanel,
  ReferencesSidePanelData,
} from './references-side-panel';

@Injectable({ providedIn: 'root' })
export class ReferencesSidePanelService {
  private readonly _sidePanelService = inject(SidePanelService);

  public open(referenceId?: string): void {
    const data: ReferencesSidePanelData = { referenceId };
    const activePanel = this._sidePanelService.activePanel();
    if (activePanel?.component === ReferencesSidePanel) {
      activePanel.data = data;
      this._sidePanelService.refresh();
      this._sidePanelService.setShowPanel(true);
      return;
    }

    this._sidePanelService.push<ReferencesSidePanel, ReferencesSidePanelData>(
      ReferencesSidePanel,
      data,
      'spec-sheet-external-references',
    );
  }
}
