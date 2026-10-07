import { Component, computed, inject } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { TagModule } from 'primeng/tag';
import { SheetComparePicker } from '../sheet-compare-picker/sheet-compare-picker';
import { momentLabel } from '../sheet-labels.util';
import { SheetPresence } from '../sheet-presence/sheet-presence';
import { SheetPublish } from '../sheet-publish/sheet-publish';
import { SheetVersionPicker } from '../sheet-version-picker/sheet-version-picker';
import { SheetStore } from '../sheet.store';

/**
 * The tools for the whole sheet: which version to look at, who else has it open, adding a table from one
 * of the sheet type's templates, publishing the user's changes, and refreshing. A past version is read-only,
 * so it only offers the picker and refresh.
 */
@Component({
  selector: 'app-sheet-toolbar',
  imports: [
    ButtonModule,
    MenuModule,
    SheetComparePicker,
    SheetPresence,
    SheetPublish,
    SheetVersionPicker,
    TagModule,
  ],
  templateUrl: './sheet-toolbar.html',
  styleUrl: './sheet-toolbar.scss',
})
export class SheetToolbar {
  public readonly canEdit = computed(() => this.store.canEdit());

  public readonly isBusy = computed(() => this.store.isBusy());

  public readonly templateMenu = computed<MenuItem[]>(() =>
    (this.store.sheet()?.availableTemplates ?? []).map((template) => ({
      label: template.name,
      command: () => void this.store.addTable(template.id),
    })),
  );

  public readonly hasTemplates = computed(() => this.templateMenu().length > 0);

  /** The read-only notice for a past view, naming the version or moment being looked at. */
  public readonly readOnlyLabel = computed(() => {
    const sheet = this.store.sheet();
    if (sheet === null || sheet.isLive) {
      return null;
    }
    if (sheet.viewedVersionNumber !== null) {
      return `Read-only: Version ${sheet.viewedVersionNumber}`;
    }
    return sheet.viewedAsOfUtc ? `Read-only: ${momentLabel(sheet.viewedAsOfUtc)}` : null;
  });

  private readonly store = inject(SheetStore);

  public reload(): void {
    this.store.reload();
  }
}
