import { Component, computed, inject } from '@angular/core';
import { SheetApprovalsMenu } from '../sheet-approvals-menu/sheet-approvals-menu';
import { SheetPresence } from '../sheet-presence/sheet-presence';
import { SheetPublish } from '../sheet-publish/sheet-publish';
import { SheetSearch } from '../sheet-search/sheet-search';
import { SheetStatusSummary } from '../sheet-status-summary/sheet-status-summary';
import { SheetViewMenu } from '../sheet-view-menu/sheet-view-menu';
import { SheetStore } from '../sheet.store';

/**
 * The tools for the whole sheet, at the right of its top line: searching it, what is being looked at, what is waiting
 * to be published, what waits for the user's approval, who else has it open, and publishing the user's changes. A past version is read-only,
 * so it only offers the view and who is there.
 */
@Component({
  selector: 'app-sheet-toolbar',
  imports: [SheetApprovalsMenu, SheetPresence, SheetPublish, SheetSearch, SheetStatusSummary, SheetViewMenu],
  templateUrl: './sheet-toolbar.html',
  styleUrl: './sheet-toolbar.scss',
})
export class SheetToolbar {
  public readonly canEdit = computed(() => this.store.canEdit());

  private readonly store = inject(SheetStore);
}
