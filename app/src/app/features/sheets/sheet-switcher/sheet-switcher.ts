import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TabsModule } from 'primeng/tabs';
import { SheetType } from '../../../core/models/sheet-type.model';
import { sheetTypeIcon } from '../../../shared/sheet-type-icon';
import { SheetTab } from '../models/sheet-tab.model';

/** PrimeNG tabs for the phase's sheets; each tab navigates to that sheet's own route. */
@Component({
  selector: 'app-sheet-switcher',
  imports: [RouterLink, TabsModule],
  templateUrl: './sheet-switcher.html',
  styleUrl: './sheet-switcher.scss',
})
export class SheetSwitcher {
  public readonly sheetTypes = input.required<SheetType[]>();
  public readonly phaseId = input.required<number>();
  public readonly currentId = input.required<number>();

  public readonly tabs = computed<SheetTab[]>(() =>
    this.sheetTypes().map((sheetType) => ({
      id: sheetType.id,
      name: sheetType.name,
      icon: sheetTypeIcon(sheetType.name),
      link: ['/phases', this.phaseId(), 'sheets', sheetType.id],
    })),
  );
}
