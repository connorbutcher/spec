import { computed, inject } from '@angular/core';
import { CellLookupService } from '../../../core/services/cell-lookup.service';
import { PUSpecSheetTableCellBase } from '../../sheet/models/cells/pu-spec-sheet-table-cell-base';
import { SpecSheetState } from './spec-sheet-state';

export class TableCellComparison {
  public readonly state: SpecSheetState = 'unchanged';
  public readonly cellType = computed(() => this.tableCell.tableCellType());
  public readonly tableCell: PUSpecSheetTableCellBase;

  private readonly _cellLookupService = inject(CellLookupService);

  constructor(
    public readonly left: PUSpecSheetTableCellBase | null,
    public readonly right: PUSpecSheetTableCellBase | null,
  ) {
    this.tableCell = left ?? right!;

    if (!left && right) {
      this.state = 'added';
      this._cellLookupService.registerCell(right.referenceId(), right);
    } else if (left && !right) {
      this.state = 'removed';
      this._cellLookupService.registerCell(left.referenceId(), left);
    } else if (
      left &&
      right &&
      (left.cellValue as any).value !== (right.cellValue as any).value
    ) {
      this.state = 'modified';
      this._cellLookupService.registerCell(right.referenceId(), right);
    } else {
      this._cellLookupService.registerCell(right!.referenceId(), right);
    }
  }
}
