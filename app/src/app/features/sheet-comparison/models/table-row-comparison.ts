import { inject } from '@angular/core';
import { uniq } from 'lodash';
import { CellLookupService } from '../../../core/services/cell-lookup.service';
import { PUSpecSheetTableRow } from '../../sheet/models/pu-spec-sheet-table-row';
import { SpecSheetState } from './spec-sheet-state';
import { TableCellComparison } from './table-cell-comparison';

export class TableRowComparison {
  public readonly cells: TableCellComparison[] = [];
  public readonly state: SpecSheetState = 'unchanged';

  private readonly _cellLookupService = inject(CellLookupService);

  constructor(
    public readonly left: PUSpecSheetTableRow | null,
    public readonly right: PUSpecSheetTableRow | null,
  ) {
    if (!left && right) {
      this.state = 'added';
      this._cellLookupService.registerRow(right.referenceId(), right);
    } else if (left && !right) {
      this.state = 'removed';
      this._cellLookupService.registerRow(left.referenceId(), left);
    } else if (left && right && left.rowNumber() !== right.rowNumber()) {
      this.state = 'modified';
      this._cellLookupService.registerRow(right.referenceId(), right);
    } else {
      this._cellLookupService.registerRow(right!.referenceId(), right);
    }

    this.buildComparisons();
  }

  public get states(): SpecSheetState[] {
    return uniq(this.cells.map((c) => c.state));
  }

  private buildComparisons(): void {
    for (const leftCell of this.left?.cells() || []) {
      const matchingRightCell = this.right
        ?.cells()
        .find((c) => c.referenceId() === leftCell.referenceId());
      this.cells.push(
        new TableCellComparison(leftCell, matchingRightCell || null),
      );
    }

    for (const rightCell of this.right?.cells() || []) {
      const hasMatchingLeftCell = this.cells.some(
        (c) =>
          c.left !== null && c.left.referenceId() === rightCell.referenceId(),
      );
      if (!hasMatchingLeftCell) {
        this.cells.push(new TableCellComparison(null, rightCell));
      }
    }
  }
}
