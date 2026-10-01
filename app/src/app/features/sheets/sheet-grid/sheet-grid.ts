import { Component, computed, input } from '@angular/core';
import { TemplateLayout } from '../../templates/models/template-layout.model';
import { SheetGridSection } from '../sheet-grid-section/sheet-grid-section';
import { layoutSheetTable } from '../sheet-layout.util';
import { SheetTable } from '../models/sheet-table.model';

/** One sheet table as a CSS grid, laid out with the template layout util so it matches its template. */
@Component({
  selector: 'app-sheet-grid',
  imports: [SheetGridSection],
  templateUrl: './sheet-grid.html',
  styleUrl: './sheet-grid.scss',
})
export class SheetGrid {
  public readonly table = input.required<SheetTable>();

  public readonly layout = computed<TemplateLayout>(() => layoutSheetTable(this.table()));

  public readonly label = computed(() => this.table().title || this.table().templateName);
}
