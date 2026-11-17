import {
  AfterViewInit,
  Component,
  computed,
  inject,
  input,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { TableGroupComparison } from '../../../../models/table-group-comparison';
import { SheetComparisonFilterService } from '../../../../services/sheet-comparison-filter.service';
import { HorizontalSheetTableRowComparison } from '../horizontal-sheet-table-row-comparison/horizontal-sheet-table-row-comparison';

@Component({
  selector: 'div[app-horizontal-sheet-table-group-comparison]',
  templateUrl: './horizontal-sheet-table-group-comparison.html',
  styleUrl: './horizontal-sheet-table-group-comparison.scss',
  host: {
    '[style.grid-area]': 'gridArea()',
  },
})
export class HorizontalSheetTableGroupComparison implements AfterViewInit {
  public readonly comparison = input.required<TableGroupComparison>();
  public readonly side = input.required<'left' | 'right'>();
  public readonly gridArea = input<string | undefined>();
  public readonly tableGroup = computed(() =>
    this.side() === 'left' ? this.comparison().left : this.comparison().right,
  );
  public readonly groupContent = viewChild('groupContent', {
    read: ViewContainerRef,
  });

  private readonly _filterService = inject(SheetComparisonFilterService);

  ngAfterViewInit(): void {
    this.buildRow();
  }

  private buildRow(): void {
    const side = this.side();
    const comparison = this.comparison();
    if (!comparison) {
      return;
    }

    const rowContent = this.groupContent();
    if (!rowContent) {
      return;
    }

    rowContent.clear();

    for (const row of comparison.rows) {
      if (
        !row.states.some((s) =>
          this._filterService.selectedStates().includes(s),
        )
      ) {
        continue;
      }

      if ((side === 'left' && !row.left) || (side === 'right' && !row.right)) {
        continue;
      }

      const componentRef = rowContent.createComponent(
        HorizontalSheetTableRowComparison,
      );
      componentRef.setInput('comparison', row);
      componentRef.setInput('side', side);
    }

    let columnPosition = 2;
    for (const childGroup of comparison.groups) {
      if (
        !childGroup.states.some((s) =>
          this._filterService.selectedStates().includes(s),
        )
      ) {
        continue;
      }

      if (
        (side === 'left' && !childGroup.left) ||
        (side === 'right' && !childGroup.right)
      ) {
        continue;
      }

      const componentRef = rowContent.createComponent(
        HorizontalSheetTableGroupComparison,
      );
      componentRef.setInput('comparison', childGroup);
      componentRef.setInput('side', side);
      componentRef.setInput(
        'gridArea',
        `1 / ${columnPosition} / span ${childGroup.rows.length} / span ${childGroup.columnDefinitions().count}`,
      );

      columnPosition += childGroup.columnDefinitions().count;
    }
  }
}
