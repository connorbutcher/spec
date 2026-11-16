import { Component, computed, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { Phase } from '../../../core/models/phase.model';
import { SheetSummary } from '../../../core/models/sheet-summary.model';
import { SheetType } from '../../../core/models/sheet-type.model';
import { SheetTypeIcon } from '../../../shared/sheet-type-icon';
import { PhaseSheetRow } from '../models/phase-sheet-row.model';
import { PhaseSheetRowsUtil } from '../phase-sheet-rows.util';

/**
 * One sheet type's sheets in a phase as a dense grid, a column per identifier. A type with identifiers
 * can have many sheets and a New button to add one; a type without has its one sheet.
 */
@Component({
  selector: 'app-phase-sheet-group',
  imports: [ButtonModule, RouterLink, TableModule, TooltipModule],
  templateUrl: './phase-sheet-group.html',
  styleUrl: './phase-sheet-group.scss',
})

export class PhaseSheetGroup {
  public readonly phase = input.required<Phase>();
  public readonly sheetType = input.required<SheetType>();
  public readonly sheets = input.required<SheetSummary[]>();
  public readonly canCreate = input(false);

  /** The user asked to create a sheet of this type. */
  public readonly newSheet = output<SheetType>();

  public readonly icon = computed(() => SheetTypeIcon.sheetTypeIcon(this.sheetType().name));

  public readonly hasIdentifiers = computed(() => this.sheetType().identifiers.length > 0);

  public readonly rows = computed<PhaseSheetRow[]>(() =>
    PhaseSheetRowsUtil.rows(this.phase(), this.sheetType(), this.sheets()),
  );

  /** Columns: #, the name or one per identifier, then version, status and last updated. */
  public readonly columnCount = computed(() => 4 + Math.max(1, this.sheetType().identifiers.length));
}
