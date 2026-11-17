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
  selector: 'div[app-horizontal-sheet-table-row-comparison]',
  templateUrl: './horizontal-sheet-table-row-comparison.html',
  styleUrl: './horizontal-sheet-table-row-comparison.scss',
  host: {
    '[style.grid-row]': 'tableRow()?.rowNumber()',
  },
})
export class HorizontalSheetTableRowComparison implements AfterViewInit {
  public readonly comparison = input.required<TableRowComparison>();
  public readonly side = input.required<'left' | 'right'>();
  public readonly tableRow = computed(() =>
    this.side() === 'left' ? this.comparison().left : this.comparison().right,
  );

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
