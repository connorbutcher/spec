import { Component, computed } from '@angular/core';
import { TableValuesDropdownCellProperty } from '../../../../../sheet/features/cell-properties/models/table-values-dropdown-cell-property.js';
import { PUSpecSheetTableCellGroupLookup } from '../../../../../sheet/models/cells/pu-spec-sheet-table-cell-group-lookup.js';
import { CellComparisonBase } from '../cell-comparison-base.js';

@Component({
  selector: 'app-engine-spec-dropdown-cell-comparison',
  templateUrl: './engine-spec-dropdown-cell-comparison.html',
  styleUrl: './engine-spec-dropdown-cell-comparison.scss',
})
export class EngineSpecDropdownCellComparison extends CellComparisonBase<
  PUSpecSheetTableCellGroupLookup,
  TableValuesDropdownCellProperty
> {
  public readonly groupValue = computed(() => {
    const cell = this.cell();
    if (!cell) {
      return null;
    }

    return cell.properties()?.table()?.title()?.toLocaleUpperCase();
  });

  public override readonly cellValue = computed(() => {
    const cell = this.cell();
    if (!cell) {
      return null;
    }

    return cell.value()?.value();
  });
}
