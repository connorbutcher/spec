import { Component, computed } from '@angular/core';
import { TableValuesDropdownCellProperty } from '../../../../../sheet/features/cell-properties/models/table-values-dropdown-cell-property.js';
import { PUSpecSheetTableCellGroupLookup } from '../../../../../sheet/models/cells/pu-spec-sheet-table-cell-group-lookup';
import { CellComparisonBase } from '../cell-comparison-base';

@Component({
  selector: 'app-table-dropdown-cell-comparison',
  templateUrl: './table-dropdown-cell-comparison.html',
  styleUrl: './table-dropdown-cell-comparison.scss',
})
export class TableDropdownCellComparison extends CellComparisonBase<
  PUSpecSheetTableCellGroupLookup,
  TableValuesDropdownCellProperty
> {
  public override readonly cellValue = computed(() => {
    const cell = this.cell();
    if (!cell) {
      return null;
    }

    return cell.value()?.value();
  });
}
