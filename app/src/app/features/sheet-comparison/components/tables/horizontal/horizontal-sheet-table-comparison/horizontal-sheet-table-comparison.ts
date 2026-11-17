import {
  Component,
  computed,
  effect,
  inject,
  input,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { TableComparison } from '../../../../models/table-comparison';
import { SheetComparisonFilterService } from '../../../../services/sheet-comparison-filter.service';
import { SheetTableHeaderComparison } from '../../sheet-table-header-comparison/sheet-table-header-comparison';
import { HorizontalSheetTableGroupComparison } from '../horizontal-sheet-table-group-comparison/horizontal-sheet-table-group-comparison';

@Component({
  selector: 'app-horizontal-sheet-table-comparison',
  imports: [SheetTableHeaderComparison],
  templateUrl: './horizontal-sheet-table-comparison.html',
  styleUrl: './horizontal-sheet-table-comparison.scss',
})
export class HorizontalSheetTableComparison {
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
  public readonly columnDefinitionString = computed(() => {
    return this._columnDefinitions().definitionString;
  });

  private readonly _columnDefinitions = computed(() => {
    const table = this.table();
    if (!table) {
      return { count: 1, definitionString: '1fr' };
    }

    return table.groups()[0].columnDefinitions();
  });

  private readonly _filterService = inject(SheetComparisonFilterService);

  constructor() {
    effect(() => {
      const tableComparison = this.tableComparison();
      const tableContent = this.gridContent();
      if (!tableComparison || !tableContent) {
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
        HorizontalSheetTableGroupComparison,
      );
      tableGroupComponentRef.setInput('comparison', group);
      tableGroupComponentRef.setInput('side', side);
      tableGroupComponentRef.setInput(
        'gridArea',
        `1 / 1 / span ${group.rows.length} / span ${this._columnDefinitions().count}`,
      );
    }
  }
}
