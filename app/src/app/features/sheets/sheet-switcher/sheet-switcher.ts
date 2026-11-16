import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TabsModule } from 'primeng/tabs';
import { SheetSummary } from '../../../core/models/sheet-summary.model';
import { SheetType } from '../../../core/models/sheet-type.model';
import { SheetIdentityUtil } from '../../../shared/sheet-identity.util';
import { SheetTypeIcon } from '../../../shared/sheet-type-icon';
import { SheetTab } from '../models/sheet-tab.model';

/**
 * PrimeNG tabs for the phase's sheet types; each tab navigates to a sheet of that type. A type without
 * identifiers opens its one sheet. A type with identifiers opens the first of its sheets, or the phase's
 * list of sheets when it has none yet.
 */
@Component({
  selector: 'app-sheet-switcher',
  imports: [RouterLink, TabsModule],
  templateUrl: './sheet-switcher.html',
  styleUrl: './sheet-switcher.scss',
})

export class SheetSwitcher {
  public readonly sheetTypes = input.required<SheetType[]>();
  /** The phase's sheets, of every type. */
  public readonly sheets = input<SheetSummary[]>([]);
  public readonly phaseId = input.required<number>();
  public readonly currentId = input.required<number>();

  public readonly tabs = computed<SheetTab[]>(() =>
    this.sheetTypes().map((sheetType) => ({
      id: sheetType.id,
      name: sheetType.name,
      icon: SheetTypeIcon.sheetTypeIcon(sheetType.name),
      link: this.link(sheetType),
    })),
  );

  private link(sheetType: SheetType): (string | number)[] {
    if (sheetType.identifiers.length === 0) {
      return ['/phases', this.phaseId(), 'sheets', sheetType.id];
    }
    const first = this.sheets().find((sheet) => sheet.sheetTypeId === sheetType.id);
    return first ? SheetIdentityUtil.link(first) : ['/phases', this.phaseId()];
  }
}
