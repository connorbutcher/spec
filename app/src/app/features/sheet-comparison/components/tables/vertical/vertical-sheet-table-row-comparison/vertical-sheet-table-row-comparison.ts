import {
  AfterViewInit,
  Component,
  computed,
  input,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { TableRowComparison } from '../../../../models/table-row-comparison';
import { SheetTableCellComparison } from '../../sheet-table-cell-comparison/sheet-table-cell-comparison';

@Component({
  selector: 'div[app-vertical-sheet-table-row-comparison]',
  templateUrl: './vertical-sheet-table-row-comparison.html',
  styleUrl: './vertical-sheet-table-row-comparison.scss',
  host: {
    '[style.grid-row]': 'rowNumber()',
  },
})
export class VerticalSheetTableRowComparison implements AfterViewInit {
  public readonly comparison = input.required<TableRowComparison>();
  public readonly side = input.required<'left' | 'right'>();
  public readonly tableRow = computed(() => {
    const side =
      this.side() === 'left' ? this.comparison().left : this.comparison().right;

    return side;
  });
  public readonly rowNumber = computed(() => {
    const row = this.tableRow();
    if (row) {
      return row.rowNumber();
    }

    const side = this.side();
    const comparison = this.comparison();
    const tableRow = side === 'left' ? comparison.right : comparison.left; // Get the opposite.
    return tableRow?.rowNumber();
  });

  public readonly rowContent = viewChild('rowContent', {
    read: ViewContainerRef,
  });

  ngAfterViewInit(): void {
    this.buildRow();
  }

  private buildRow(): void {
    const side = this.side();
    const comparison = this.comparison();
    if (!comparison) {
      return;
    }

    const rowContent = this.rowContent();
    if (!rowContent) {
      return;
    }

    rowContent.clear();

    for (const cell of comparison.cells) {
      const componentRef = rowContent.createComponent(SheetTableCellComparison);
      componentRef.setInput('comparison', cell);
      componentRef.setInput('side', side);
    }
  }
}
