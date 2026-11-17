import { NgClass } from '@angular/common';
import {
  AfterViewInit,
  Component,
  computed,
  input,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { FragmentedElementDirective } from '../../../../../core/directives/fragmented-element.directive';
import { TableCellComparison } from '../../../models/table-cell-comparison';
import { CellTypeHelpers } from '../cell-types/helpers/cell-type-helpers';

@Component({
  selector: 'div[app-sheet-table-cell-comparison]',
  imports: [NgClass, FragmentedElementDirective],
  templateUrl: './sheet-table-cell-comparison.html',
  styleUrl: './sheet-table-cell-comparison.scss',
  host: {
    '[style.grid-column]':
      'cell()?.columnNumber() + " / span " + (cell()?.colSpan() || 1)',
  },
})
export class SheetTableCellComparison implements AfterViewInit {
  public readonly comparison = input.required<TableCellComparison>();
  public readonly side = input.required<'left' | 'right'>();
  public readonly cell = computed(() =>
    this.side() === 'left' ? this.comparison().left : this.comparison().right,
  );
  public readonly cellType = computed(() => this.comparison().cellType());

  public readonly cellContent = viewChild('cellContent', {
    read: ViewContainerRef,
  });

  ngAfterViewInit(): void {
    this.buildRow();
  }

  private buildRow(): void {
    const cell = this.cell();
    const cellType = this.cellType();
    if (!cell || !cellType) {
      return;
    }

    const cellContent = this.cellContent();
    if (!cellContent) {
      return;
    }

    cellContent.clear();

    const component = CellTypeHelpers.getCellTypeComponent(cellType.key);
    if (!component) {
      return;
    }

    const componentRef = cellContent.createComponent(component);
    componentRef.setInput('cell', cell);
    componentRef.setInput('comparison', this.comparison());
  }
}
