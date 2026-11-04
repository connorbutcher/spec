import { Component, computed, inject, input } from '@angular/core';
import { SectionLayout } from '../../templates/models/section-layout.model';
import { SheetGridCell } from '../sheet-grid-cell/sheet-grid-cell';
import { CellLayout } from '../../templates/models/cell-layout.model';
import { RowLayout } from '../models/row-layout.model';
import { SectionTone } from '../models/section-tone';
import { SheetCell } from '../models/sheet-cell.model';
import { SheetChange } from '../models/sheet-change.model';
import { SheetRow } from '../models/sheet-row.model';
import { SheetAnchor } from '../sheet-anchor.directive';
import { SheetChangeTag } from '../sheet-change-tag/sheet-change-tag';
import { SheetIndexUtil } from '../sheet-index.util';
import { SheetRowStatusCell } from '../sheet-row-status-cell/sheet-row-status-cell';
import { SheetStatusColumnUtil } from '../sheet-status-column.util';
import { SheetStatusHeading } from '../sheet-status-heading/sheet-status-heading';
import { SheetStore } from '../sheet.store';

/**
 * A section copy on the sheet grid. It draws nothing of its own: the host is an invisible subgrid that
 * takes the section's place in its parent, so its rows and cells line up with the table. Clicking a bare
 * part of it selects the section; the selected section gets a thin outline.
 *
 * To assistive technology a top-level section is a group of rows. A section inside another has no role
 * of its own, so its rows belong to the group around it.
 */
@Component({
  selector: 'app-sheet-grid-section',
  imports: [SheetAnchor, SheetChangeTag, SheetGridCell, SheetRowStatusCell, SheetStatusHeading],
  templateUrl: './sheet-grid-section.html',
  styleUrl: './sheet-grid-section.scss',
  host: {
    '[style]': 'layout().style',
    '[attr.role]': 'role()',
    '[class.selected]': 'isSelected()',
    '[class.group]': 'isGroup()',
    '[class.nested]': 'isNested()',
    '(click)': 'select($event)',
  },
})

export class SheetGridSection {
  public readonly layout = input.required<SectionLayout>();

  /** Where the status column's heading sits, in the header section. */
  public readonly headingStyle = SheetStatusColumnUtil.statusHeadingStyle();

  /** The section's rows, each with its cells and the place of its status in the status column. */
  public readonly rows = computed<RowLayout[]>(() => {
    const cells = new Map<number, CellLayout[]>();
    for (const cell of this.layout().cells) {
      cells.set(cell.rowId, [...(cells.get(cell.rowId) ?? []), cell]);
    }
    return this.layout().section.rows.map((row, index) => ({
      rowId: row.id,
      cells: cells.get(row.id) ?? [],
      statusStyle: SheetStatusColumnUtil.rowStatusStyle(index),
    }));
  });

  /** The header's rows are captions, not data: the status column shows its heading beside them. */
  public readonly isHeader = computed(() => this.layout().section.role === 'Header');

  public readonly role = computed(() => (this.isNested() ? 'none' : 'rowgroup'));

  public readonly isSelected = computed(() => {
    const selection = this.store.selection();
    return selection?.sectionId === this.layout().section.id && selection.rowId === null;
  });

  /** The sheet's copy of this section, which knows what can be added to it. */
  public readonly sheetSection = computed(
    () => this.store.index().sections.get(this.layout().section.id) ?? null,
  );

  /** The sheet's copy of each of the section's own rows, by id, for its cells. */
  public readonly sheetRows = computed<ReadonlyMap<number, SheetRow>>(
    () => new Map((this.sheetSection()?.rows ?? []).map((row) => [row.id, row])),
  );

  /** The sheet's copy of each cell in the section's own rows, by id. */
  public readonly sheetCells = computed<ReadonlyMap<number, SheetCell>>(
    () =>
      new Map(
        (this.sheetSection()?.rows ?? []).flatMap((row) =>
          row.cells.map((cell): [number, SheetCell] => [cell.id, cell]),
        ),
      ),
  );

  /** The fill of the section's cells. Worked out once here, not by each cell. */
  public readonly tone = computed<SectionTone>(() =>
    SheetIndexUtil.toneOfSection(this.store.index(), this.layout().section.id),
  );

  /** A group holds other sections; it gets a bar and boundary around it and everything inside. */
  public readonly isGroup = computed(() => {
    const section = this.sheetSection();
    return section !== null && SheetIndexUtil.isGroupSection(section);
  });

  public readonly isNested = computed(
    () => SheetIndexUtil.sectionAncestors(this.store.index(), this.layout().section.id).length > 0,
  );

  /** What changed directly in this section, if that was after the compared version. */
  public readonly change = computed<SheetChange | null>(() =>
    this.store.markedChange(this.sheetSection()?.lastChange),
  );

  private readonly store = inject(SheetStore);

  public select(event: Event): void {
    event.stopPropagation();
    this.store.selectSection(this.layout().section.id);
  }
}
