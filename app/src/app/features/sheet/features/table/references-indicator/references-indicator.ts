import { Component, inject, input } from '@angular/core';
import { Tooltip } from 'primeng/tooltip';
import { ReferencesSidePanelService } from '../../references/references-side-panel/references-side-panel.service';
import { SheetExternalReference } from '../../../../../core/models/domain/sheets/sheet-external-reference';

@Component({
  selector: 'app-references-indicator',
  imports: [Tooltip],
  templateUrl: './references-indicator.html',
  styleUrl: './references-indicator.scss',
  host: {
    '[class.show-line]': 'showLine() && (references()?.length ?? 0) > 0',
  },
})
export class ReferencesIndicator {
  public readonly references = input.required<SheetExternalReference[]>();
  public readonly showLine = input.required<boolean>();

  private readonly _referencesSidePanel = inject(ReferencesSidePanelService);

  public viewSystemReferences(): void {
    const reference = this.references()?.[0];
    if (reference) {
      this._referencesSidePanel.open(reference.referenceId);
    }
  }
}
