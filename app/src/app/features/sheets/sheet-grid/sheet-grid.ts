import { Component, computed, input, linkedSignal } from '@angular/core';
import { reuseUnchanged } from '../../../shared/reuse-unchanged.util';
import { TemplateLayout } from '../../templates/models/template-layout.model';
import { SheetGridColumnBlock } from '../sheet-grid-column-block/sheet-grid-column-block';
import { SheetGridSection } from '../sheet-grid-section/sheet-grid-section';
import { tableLabel } from '../sheet-labels.util';
import { layoutSheetTable } from '../sheet-layout.util';
import { SheetTable } from '../models/sheet-table.model';

/** One sheet table as a CSS grid, laid out with the template layout util so it matches its template. */
@Component({
  selector: 'app-sheet-grid',
  imports: [SheetGridColumnBlock, SheetGridSection],
  templateUrl: './sheet-grid.html',
  styleUrl: './sheet-grid.scss',
})
export class SheetGrid {
  public readonly table = input.required<SheetTable>();

  /**
   * Where everything in the table sits. Each new layout keeps the parts of the last one that did not
   * move, so a change to a value, which moves nothing, re-renders no section or cell from here.
   */
  public readonly layout = linkedSignal<SheetTable, TemplateLayout>({
    source: this.table,
    computation: (table, previous) => reuseUnchanged(previous?.value, layoutSheetTable(table)),
  });

  public readonly label = computed(() => tableLabel(this.table()));
}
