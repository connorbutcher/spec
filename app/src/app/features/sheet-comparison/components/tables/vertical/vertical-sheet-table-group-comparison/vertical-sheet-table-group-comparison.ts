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
import { VerticalSheetTableRowComparison } from '../vertical-sheet-table-row-comparison/vertical-sheet-table-row-comparison';

@Component({
  selector: 'div[app-vertical-sheet-table-group-comparison]',
  templateUrl: './vertical-sheet-table-group-comparison.html',
  styleUrl: './vertical-sheet-table-group-comparison.scss',
})
export class VerticalSheetTableGroupComparison implements AfterViewInit {
  public readonly comparison = input.required<TableGroupComparison>();
  public readonly side = input.required<'left' | 'right'>();
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

      const componentRef = rowContent.createComponent(
        VerticalSheetTableRowComparison,
      );
      componentRef.setInput('comparison', row);
      componentRef.setInput('side', side);
    }

    for (const childGroup of comparison.groups) {
      if (
        !childGroup.states.some((s) =>
          this._filterService.selectedStates().includes(s),
        )
      ) {
        continue;
      }

      const componentRef = rowContent.createComponent(
        VerticalSheetTableGroupComparison,
      );
      componentRef.setInput('comparison', childGroup);
      componentRef.setInput('side', side);
    }
  }
}
