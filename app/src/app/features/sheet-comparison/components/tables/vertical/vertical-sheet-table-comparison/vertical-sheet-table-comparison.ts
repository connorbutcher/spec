import {
  Component,
  computed,
  effect,
  inject,
  input,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { PUSpecSheetTableRow } from '../../../../../sheet/models/pu-spec-sheet-table-row';
import { TableComparison } from '../../../../models/table-comparison';
import { SheetComparisonFilterService } from '../../../../services/sheet-comparison-filter.service';
import { SheetTableHeaderComparison } from '../../sheet-table-header-comparison/sheet-table-header-comparison';
import { VerticalSheetTableGroupComparison } from '../vertical-sheet-table-group-comparison/vertical-sheet-table-group-comparison';

@Component({
  selector: 'app-vertical-sheet-table-comparison',
  imports: [SheetTableHeaderComparison],
  templateUrl: './vertical-sheet-table-comparison.html',
  styleUrl: './vertical-sheet-table-comparison.scss',
})
export class VerticalSheetTableComparison {
  public readonly tableComparison = input.required<TableComparison>();
  public readonly side = input.required<'left' | 'right'>();
  public readonly table = computed(() =>
    this.side() === 'left'
      ? this.tableComparison().left
      : this.tableComparison().right,
  );
  public readonly gridContent = viewChild('gridContent', {
    read: ViewContainerRef,
  });
  public readonly columnCount = computed(() => {
    const table = this.table();
    if (!table) {
      return 1;
    }

    // Get the row with the most cells
    let row: PUSpecSheetTableRow | undefined = undefined;
    for (const defaultGroup of table.groups()) {
      for (const rowTemplate of defaultGroup.rows()) {
        if (rowTemplate.cells().length > (row ? row.cells().length : 0)) {
          row = rowTemplate;
        }
      }
    }

    let columnDefinitions = '';
    for (const cell of row!.cells()) {
      const width = cell.stylingConfiguration().width;
      columnDefinitions += `minmax(${width}em, auto) `;
    }

    return columnDefinitions.trim();
  });

  private readonly _filterService = inject(SheetComparisonFilterService);

  constructor() {
    effect(() => {
      const table = this.table();
      const tableContent = this.gridContent();
      if (!table || !tableContent) {
        return;
      }

      this.buildRows();
    });
  }

  private buildRows(): void {
    const comparison = this.tableComparison();
    const side = this.side();
    if (!comparison) {
      return;
    }

    const tableContent = this.gridContent();
    if (!tableContent) {
      return;
    }

    tableContent.clear();

    for (const group of comparison.groups) {
      if (
        !group.states.some((s) =>
          this._filterService.selectedStates().includes(s),
        )
      ) {
        continue;
      }

      const tableGroupComponentRef = tableContent.createComponent(
        VerticalSheetTableGroupComparison,
      );
      tableGroupComponentRef.setInput('comparison', group);
      tableGroupComponentRef.setInput('side', side);
    }
  }
}
