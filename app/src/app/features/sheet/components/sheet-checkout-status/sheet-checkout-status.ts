import { Component, computed, inject } from '@angular/core';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';
import { PUSpecSheetAvatar } from '../../../common/pu-spec-sheet-avatar/pu-spec-sheet-avatar';
import { ActiveSheetService } from '../../services/active-sheet.service';

/** The sheet's version, who has what checked out on it, and who has it open. */
@Component({
  selector: 'app-sheet-checkout-status',
  imports: [Tag, Tooltip, PUSpecSheetAvatar],
  templateUrl: './sheet-checkout-status.html',
  styleUrl: './sheet-checkout-status.scss',
})
export class SheetCheckoutStatus {
  public readonly sheetId = computed(
    () => this._activeSheetService.specSheet().id,
  );
  /** The latest published version, which the checked-out changes are made over. */
  public readonly versionLabel = computed(() => {
    const versionNumber =
      this._activeSheetService.specSheet().sheet.version?.versionNumber;
    return versionNumber ? `v${versionNumber}` : 'Not yet published';
  });
  public readonly isCurrent = computed(() =>
    this._activeSheetService.canEdit(),
  );
  public readonly summary = computed(() =>
    this._activeSheetService.checkoutSummary(),
  );
  public readonly othersLabel = computed(() => {
    const others = this.summary().others;
    const total = others.reduce((sum, other) => sum + other.count, 0);
    return total === 1 ? '1 by others' : `${total} by others`;
  });
  public readonly othersTooltip = computed(() =>
    this.summary()
      .others.map((other) => `${other.userName}: ${other.count}`)
      .join('\n'),
  );

  private readonly _activeSheetService = inject(ActiveSheetService);
}
