import { Component, computed, input } from '@angular/core';
import { TooltipModule } from 'primeng/tooltip';
import { SheetChange } from '../models/sheet-change.model';
import { changeLabel } from '../sheet-labels.util';

/**
 * The small marks drawn over a cell: a corner triangle for a value that changed since the compared
 * version, and a warning sign for a cell that needs attention. Each explains itself in a tooltip.
 * What is true of the whole row (its version, who has it) is in the status column instead.
 */
@Component({
  selector: 'app-sheet-cell-marks',
  imports: [TooltipModule],
  templateUrl: './sheet-cell-marks.html',
  styleUrl: './sheet-cell-marks.scss',
})
export class SheetCellMarks {
  /** The change to the cell's own value. */
  public readonly cellChange = input<SheetChange | null>(null);
  /** Something about the cell that needs a person's attention, in words. */
  public readonly warning = input<string | null>(null);

  public readonly cellDetail = computed(() => {
    const change = this.cellChange();
    return change === null ? null : changeLabel(change);
  });
}
