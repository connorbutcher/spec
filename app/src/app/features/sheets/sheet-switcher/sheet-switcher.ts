import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SheetType } from '../../../core/models/sheet-type.model';
import { sheetTypeIcon } from '../../../shared/sheet-type-icon';

/** A compact strip of the phase's other sheets, to jump between them without going back. */
@Component({
  selector: 'app-sheet-switcher',
  imports: [RouterLink],
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
