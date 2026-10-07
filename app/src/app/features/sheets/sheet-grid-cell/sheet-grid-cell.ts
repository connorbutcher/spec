import {
  afterNextRender,
  Component,
  computed,
  ElementRef,
  inject,
  Injector,
  input,
} from '@angular/core';
import { effectiveConfiguration, effectiveStyle } from '../../templates/cell-settings.util';
import { cellStyleCss } from '../../templates/cell-style-css.util';
import { isDisplayOnly } from '../../templates/models/cell-kinds';
import { CellLayout } from '../../templates/models/cell-layout.model';
import { CellType } from '../../templates/models/cell-type.model';
import { GridStyle } from '../../templates/models/grid-style';
import { CellValueRequest } from '../models/cell-value-request.model';
import { EditableCell } from '../models/editable-cell.model';
import { SectionTone } from '../models/section-tone';
import { SheetCell } from '../models/sheet-cell.model';
import { SheetChange } from '../models/sheet-change.model';
import { SheetRow } from '../models/sheet-row.model';
import { SheetCellEditor } from '../sheet-cell-editor/sheet-cell-editor';
import { SheetCellMarks } from '../sheet-cell-marks/sheet-cell-marks';
import { SheetCellValue } from '../sheet-cell-value/sheet-cell-value';
import { sectionTone } from '../sheet-index.util';
import { otherUsersLock } from '../sheet-lock.util';
import { SheetStore } from '../sheet.store';

const FLEX_ALIGNMENT: Readonly<Record<string, string>> = {
  left: 'flex-start',
  center: 'center',
  right: 'flex-end',
};

/**
 * A cell on the sheet grid, placed by the template layout. It behaves like a table cell: heading and
 * group cells show their caption; a value cell shows its value, and swaps it for a control of its kind,
 * filling the whole cell, when the user clicks or tabs into it, and back again as soon as they click
 * or tab away. Only one cell holds a control at a time, so opening a sheet doesn't build one per cell. Moving into a control only selects the row; the
 * row is locked to the user, and held until they publish, by the first change that makes a real
 * difference to what is published. A row locked by someone else, or a past version, only ever shows
 * plain values, with a lock mark on the row's first cell.
 *
 * This component decides what the cell is and where it sits; the editor, the plain value and the
 * marks over the cell are each their own component.
 */
@Component({
  selector: 'app-sheet-grid-cell',
  imports: [SheetCellEditor, SheetCellMarks, SheetCellValue],
  templateUrl: './sheet-grid-cell.html',
  styleUrl: './sheet-grid-cell.scss',
  host: {
    '[attr.role]': 'role()',
    '[style]': 'hostStyle()',
    '[class.selected]': 'isSelected()',
    '[class.mine]': 'isMine()',
    '[class.theirs]': 'lockedByOther()',
    '[class.display-only]': 'displayOnly()',
    '[class.editable]': 'isEditable()',
    '[attr.data-tone]': 'tone()',
    '[attr.tabindex]': 'tabIndex()',
    '(pointerdown)': 'startEditing(true)',
    '(focus)': 'startEditing(false)',
    '(click)': 'select($event)',
    '(keydown.enter)': 'select($event)',
  },
})
export class SheetGridCell {
  public readonly layout = input.required<CellLayout>();

  public readonly cell = computed<SheetCell | null>(
    () => this.store.index().cells.get(this.layout().cell.id) ?? null,
  );

  public readonly row = computed<SheetRow | null>(
    () => this.store.index().rows.get(this.layout().rowId) ?? null,
  );

  public readonly cellType = computed<CellType | null>(
    () => this.store.cellTypes().get(this.layout().cell.cellTypeId) ?? null,
  );

  public readonly displayOnly = computed(() => isDisplayOnly(this.cellType()?.kind));

  public readonly configuration = computed(() => {
    const cellType = this.cellType();
    return cellType
      ? effectiveConfiguration(cellType.configuration, this.layout().cell.configurationOverride)
      : null;
  });

  public readonly isMine = computed(() => this.row()?.lock?.isMine === true);

  public readonly lockedByOther = computed(() => otherUsersLock(this.row()?.lock) !== null);

  /**
   * What the cell's control needs, when it has one: the cell takes a value, the sheet is live, and
   * nobody else has the row. A free row counts too: the first real change to one of its cells is what
   * locks it.
   */
  public readonly editable = computed<EditableCell | null>(() => {
    const cell = this.cell();
    const cellType = this.cellType();
    const configuration = this.configuration();
    if (!this.store.canEdit() || this.displayOnly() || this.lockedByOther()) {
      return null;
    }
    return cell !== null && cellType !== null && configuration !== null
      ? { cell, cellType, configuration }
      : null;
  });

  public readonly isEditable = computed(() => this.editable() !== null);

  /**
   * Whether the cell's control is on screen: the cell can be edited and is the one the user is in.
   * A checkbox is always its control, because the click that reaches it has to tick it.
   */
  public readonly showsEditor = computed(() => {
    const editable = this.editable();
    return (
      editable !== null &&
      (editable.cellType.kind === 'Checkbox' || this.store.editingCellId() === editable.cell.id)
    );
  });

  /** The cell's fill, from where its section sits. A background set in the cell's own style takes precedence. */
  public readonly tone = computed<SectionTone>(() =>
    sectionTone(this.store.index(), this.layout().rowId),
  );

  /** A caption in the header names its column; every other cell is an ordinary cell. */
  public readonly role = computed(() =>
    this.displayOnly() && this.tone() === 'header' ? 'columnheader' : 'gridcell',
  );

  /**
   * A cell is a tab stop so it can be moved into, or its row selected with Enter. Not while its control
   * is on screen (the control is the stop), and not in a read-only view, where neither can happen.
   */
  public readonly tabIndex = computed(() =>
    this.showsEditor() || !this.store.canEdit() ? null : 0,
  );

  /** The grid placement plus the cell's background, which fills the whole grid area. */
  public readonly hostStyle = computed<GridStyle>(() => {
    const background = this.styleCss()['background-color'];
    return background
      ? { ...this.layout().style, 'background-color': background }
      : this.layout().style;
  });

  /** The cell's text styling, with its alignment also lining up what's inside. */
  public readonly contentStyle = computed<Record<string, string>>(() => {
    const { 'background-color': _background, ...text } = this.styleCss();
    const align = text['text-align'];
    return align ? { ...text, 'align-items': FLEX_ALIGNMENT[align] ?? 'flex-start' } : text;
  });

  public readonly isSelected = computed(
    () => this.store.selection()?.rowId === this.layout().rowId,
  );

  public readonly caption = computed(() => this.layout().cell.caption ?? '');

  /** The caption doubles as the hint inside an empty control, marked if the cell is required. */
  public readonly hint = computed(() =>
    this.layout().cell.isRequired && this.caption() ? `${this.caption()} *` : this.caption(),
  );

  /** The cell's value changed after the version being compared against. */
  public readonly cellChange = computed<SheetChange | null>(() =>
    this.store.markedChange(this.cell()?.lastChange),
  );

  /** On the row's first cell: the row changed after the version being compared against. */
  public readonly rowChange = computed<SheetChange | null>(() =>
    this.isFirstInRow() ? this.store.markedChange(this.row()?.lastChange) : null,
  );

  /** On the row's first cell: who else is editing the row. Your own rows are only tinted. */
  public readonly lockedBy = computed<string | null>(() =>
    this.isFirstInRow() ? (otherUsersLock(this.row()?.lock)?.userName ?? null) : null,
  );

  public readonly hasMarks = computed(
    () => this.cellChange() !== null || this.rowChange() !== null || this.lockedBy() !== null,
  );

  private readonly store = inject(SheetStore);
  private readonly element = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly injector = inject(Injector);

  private readonly styleCss = computed<Record<string, string>>(() => {
    const cellType = this.cellType();
    return cellType
      ? cellStyleCss(effectiveStyle(cellType.style, this.layout().cell.styleOverride))
      : {};
  });

  /** Row-level marks are drawn once, on the row's first cell. */
  private readonly isFirstInRow = computed(
    () => this.row()?.cells[0]?.id === this.layout().cell.id,
  );

  /**
   * The user pressed on the cell or tabbed to it: put its control on screen and move into it, as if
   * the control had been there all along. A press also opens a dropdown, as pressing one would.
   */
  public startEditing(byPointer: boolean): void {
    const editable = this.editable();
    if (editable === null || this.showsEditor()) {
      return;
    }
    this.store.edit(editable.cell.id);
    afterNextRender(() => this.enterEditor(byPointer), { injector: this.injector });
  }

  /** The user clicked or tabbed away: back to the plain value. The control saved its value as it lost focus. */
  public stopEditing(): void {
    const cell = this.cell();
    if (cell !== null) {
      this.store.stopEditing(cell.id);
    }
  }

  public select(event: Event): void {
    event.stopPropagation();
    this.selectRow();
  }

  /** Also called when the user moves into the cell's control. Nothing is locked or changed until they change a value. */
  public selectRow(): void {
    this.store.selectRow(this.layout().rowId);
  }

  public save(request: CellValueRequest): void {
    const row = this.row();
    if (row !== null) {
      void this.store.saveValues(row.id, [request]);
    }
  }

  private enterEditor(byPointer: boolean): void {
    const host = this.element.nativeElement;
    const control = host.querySelector<HTMLElement>('input, textarea, [role="combobox"]');
    control?.focus();
    if (control instanceof HTMLInputElement || control instanceof HTMLTextAreaElement) {
      // The control writes its value into the box just after it renders, which would drop the
      // selection, so select once that has happened. The value is selected as it is when tabbing
      // into any box: typing replaces it.
      setTimeout(() => control.select());
    }
    if (byPointer) {
      host.querySelector<HTMLElement>('.p-select')?.click();
    }
  }
}
