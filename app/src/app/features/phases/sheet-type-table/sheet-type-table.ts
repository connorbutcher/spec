import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TableModule } from 'primeng/table';
import { Phase } from '../../../core/models/phase.model';
import { SheetType } from '../../../core/models/sheet-type.model';
import { sheetTypeIcon } from '../../../shared/sheet-type-icon';

/**
 * The phase's PU Spec Sheets as a dense grid. Each row opens the sheet on its own screen. Version,
 * status and last-updated come from the sheet's own version history once sheets are versioned.
 */
@Component({
  selector: 'app-sheet-type-table',
  imports: [RouterLink, TableModule],
  templateUrl: './sheet-type-table.html',
  styleUrl: './sheet-type-table.scss',
})
export class SheetTypeTable {
  public readonly sheetTypes = input.required<SheetType[]>();
  public readonly phase = input.required<Phase>();

  public icon(sheetType: SheetType): string {
    return sheetTypeIcon(sheetType.name);
  }

  public sheetLink(sheetType: SheetType): (string | number)[] {
    return ['/phases', this.phase().id, 'sheets', sheetType.id];
  }
}
