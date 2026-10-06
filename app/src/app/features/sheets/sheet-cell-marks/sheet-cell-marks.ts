import { Component, computed, input } from '@angular/core';
import { TooltipModule } from 'primeng/tooltip';
import { SheetChange } from '../models/sheet-change.model';
import { changeLabel, versionLabel } from '../sheet-labels.util';

/**
 * The small marks drawn over a cell: a bar and version for a row that changed since the compared
 * version, a corner triangle for a cell whose value changed, and a lock for a row someone else is
 * editing. Each explains itself in a tooltip.
 */
@Component({
  selector: 'app-sheet-cell-marks',
  imports: [TooltipModule],
  templateUrl: './sheet-cell-marks.html',
  styleUrl: './sheet-cell-marks.scss',
})
export class SheetCellMarks {
  /** The change to the cell's row, on the row's first cell only. */
  public readonly rowChange = input<SheetChange | null>(null);
  /** The change to the cell's own value. */
  public readonly cellChange = input<SheetChange | null>(null);
  /** Who is editing the cell's row, on the row's first cell only. */
  public readonly lockedBy = input<string | null>(null);

  public readonly rowVersion = computed(() => {
    const change = this.rowChange();
    return change === null ? null : versionLabel(change.versionNumber);
  });

  public readonly rowDetail = computed(() => {
    const change = this.rowChange();
    return change === null ? null : changeLabel(change);
  });

  public readonly cellDetail = computed(() => {
    const change = this.cellChange();
    return change === null ? null : changeLabel(change);
  });

  public readonly lockLabel = computed(() => {
    const user = this.lockedBy();
    return user === null ? null : `Being edited by ${user}`;
  });
}
