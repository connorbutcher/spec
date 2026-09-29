import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TabsModule } from 'primeng/tabs';
import { SheetType } from '../../../core/models/sheet-type.model';
import { sheetTypeIcon } from '../../../shared/sheet-type-icon';

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

  public icon(sheetType: SheetType): string {
    return sheetTypeIcon(sheetType.name);
  }

  public link(sheetType: SheetType): (string | number)[] {
    return ['/phases', this.phaseId(), 'sheets', sheetType.id];
  }
}
