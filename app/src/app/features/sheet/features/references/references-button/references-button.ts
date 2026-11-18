import { Component, inject, Signal } from '@angular/core';
import { Badge } from 'primeng/badge';
import { ButtonDirective } from 'primeng/button';
import { ActiveSheetService } from '../../../services/active-sheet.service';
import { ReferencesSidePanelService } from '../references-side-panel/references-side-panel.service';
import { SheetExternalReference } from '../../../../../core/models/domain/sheets/sheet-external-reference';

@Component({
  selector: 'app-references-button',
  imports: [ButtonDirective, Badge],
  templateUrl: './references-button.html',
  styleUrl: './references-button.scss',
})
export class ReferencesButton {
  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _referencesSidePanel = inject(ReferencesSidePanelService);

  public viewExternalReferences(): void {
    if (!this.references() || this.references().length === 0) {
      return;
    }

    this._referencesSidePanel.open();
  }

  public get references(): Signal<SheetExternalReference[]> {
    return this._activeSheetService.externalReferences;
  }
}
