import { Component, computed, inject, input } from '@angular/core';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';
import { SheetItemLockHelpers } from '../../../../../core/helpers/sheet-item-lock-helpers';
import { SheetItemRevision } from '../../../../../core/models/domain/sheets/sheet-item-revision';
import { ActiveSheetService } from '../../../services/active-sheet.service';

/** Shows who has a row or table checked out, or that its changes are awaiting approval. Nothing when it is free. */
@Component({
  selector: 'app-item-lock-tag',
  imports: [Tag, Tooltip],
  templateUrl: './item-lock-tag.html',
  styleUrl: './item-lock-tag.scss',
})
export class ItemLockTag {
  public readonly revision = input.required<SheetItemRevision | undefined>();
  public readonly lock = computed(() =>
    this._activeSheetService.lockOf(this.revision()),
  );
  public readonly label = computed(() => {
    const lock = this.lock();
    if (!lock) {
      return '';
    }

    return lock.isMine ? 'You' : lock.userName;
  });
  public readonly icon = computed(() =>
    this.lock()?.isAwaitingApproval ? 'pi pi-hourglass' : 'pi pi-lock',
  );
  public readonly severity = computed(() => {
    const lock = this.lock();
    if (lock?.isAwaitingApproval) {
      return 'warn';
    }

    return lock?.isMine ? 'info' : 'secondary';
  });
  public readonly description = computed(() => {
    const lock = this.lock();
    return lock ? SheetItemLockHelpers.describe(lock) : '';
  });

  private readonly _activeSheetService = inject(ActiveSheetService);
}
