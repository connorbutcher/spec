import { Component, computed, inject, input } from '@angular/core';
import { TooltipModule } from 'primeng/tooltip';
import { effectiveConfiguration, effectiveStyle } from '../../templates/cell-settings.util';
import { cellStyleCss } from '../../templates/cell-style-css.util';
import { isDisplayOnly } from '../../templates/models/cell-kinds';
import { CellLayout } from '../../templates/models/cell-layout.model';
import { CellType } from '../../templates/models/cell-type.model';
import { GridStyle } from '../../templates/models/grid-style';
import { CellValueRequest } from '../models/cell-value-request.model';
import { SheetCell } from '../models/sheet-cell.model';
import { SheetRow } from '../models/sheet-row.model';
import { SheetCheckboxCell } from '../sheet-checkbox-cell/sheet-checkbox-cell';
import { SheetDateCell } from '../sheet-date-cell/sheet-date-cell';
import { SheetDropdownCell } from '../sheet-dropdown-cell/sheet-dropdown-cell';
import { SheetNumberCell } from '../sheet-number-cell/sheet-number-cell';
import { SheetTextCell } from '../sheet-text-cell/sheet-text-cell';
import { changeLabel } from '../sheet-labels.util';
import { sectionAncestors } from '../sheet-index.util';
import { SheetStore } from '../sheet.store';

const FLEX_ALIGNMENT: Readonly<Record<string, string>> = {
  left: 'flex-start',
  center: 'center',
  right: 'flex-end',
};

/**
 * A cell on the sheet grid, placed by the template layout. It behaves like a table cell: heading and
 * group cells show their caption; a value cell holds a control for its kind that fills the whole cell,
 * so the user can click in and type. Moving into a control only selects the row; the row is locked to the
 * user, and held until they publish, by the first change that makes a real difference to what is published. A row locked by someone else, or a past version, shows plain
 * values instead, with a lock mark on the row's first cell.
 */
@Component({
  selector: 'app-sheet-grid-cell',
  imports: [
    SheetCheckboxCell,
    SheetDateCell,
    SheetDropdownCell,
    SheetNumberCell,
    SheetTextCell,
    TooltipModule,
  ],
  templateUrl: './sheet-grid-cell.html',
  styleUrl: './sheet-grid-cell.scss',
  host: {
    role: 'gridcell',
    '[style]': 'hostStyle()',
    '[class.selected]': 'isSelected()',
    '[class.mine]': 'isMine()',
    '[class.theirs]': 'lockedByOther()',
    '[class.display-only]': 'displayOnly()',
    '[class.editable]': 'isEditable()',
    '[attr.data-tone]': 'tone()',
    '[attr.tabindex]': 'isEditable() ? null : 0',
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

  public readonly styleCss = computed<Record<string, string>>(() => {
    const cellType = this.cellType();
    return cellType
      ? cellStyleCss(effectiveStyle(cellType.style, this.layout().cell.styleOverride))
      : {};
  });

  public readonly isMine = computed(() => this.row()?.lock?.isMine === true);

  public readonly lockedByOther = computed(() => {
    const lock = this.row()?.lock;
    return lock !== null && lock !== undefined && !lock.isMine;
  });

  /**
   * Whether the cell holds a control: it takes a value, the sheet is live, and nobody else has the row.
   * A free row counts too: the first real change to one of its cells is what locks it.
   */
  public readonly isEditable = computed(
    () =>
      this.store.canEdit() &&
      !this.displayOnly() &&
      !this.lockedByOther() &&
      this.cell() !== null &&
      this.cellType() !== null &&
      this.configuration() !== null,
  );

  /**
   * The cell's fill, from where its section sits: the header, a group (a section that holds other
   * sections) at its depth, or plain data. Deeper groups are lighter, so the tree reads at a glance. A
   * background set in the cell's own style takes precedence.
   */
  public readonly tone = computed<string>(() => {
    const index = this.store.index();
    const section = index.sections.get(index.rowSection.get(this.layout().rowId) ?? -1);
    if (section === undefined) {
      return 'plain';
    }
    if (section.role === 'Header') {
      return 'header';
    }
    const isGroup = section.sections.length > 0 || section.addableSections.length > 0;
    return isGroup ? `group-${Math.min(sectionAncestors(index, section.id).length, 2)}` : 'plain';
  });

  /** The cell's value changed after the version being compared against. */
  public readonly cellChange = computed(() => {
    const change = this.cell()?.lastChange ?? null;
    return this.store.isMarked(change) && change !== null ? changeLabel(change) : null;
  });

  /** On the row's first cell: the row changed after the version being compared against. */
  public readonly rowChange = computed(() => {
    const row = this.row();
    const change = row?.lastChange ?? null;
    const isFirst = row !== null && row.cells[0]?.id === this.layout().cell.id;
    return isFirst && this.store.isMarked(change) && change !== null
      ? { version: `v${change.versionNumber}`, label: changeLabel(change) }
      : null;
  });

  public readonly isSelected = computed(
    () => this.store.selection()?.rowId === this.layout().rowId,
  );

  /** The lock marker goes on the first cell of a row someone else is editing. Your own rows are only tinted. */
  public readonly showsLock = computed(() => {
    const row = this.row();
    return row !== null && this.lockedByOther() && row.cells[0]?.id === this.layout().cell.id;
  });

  public readonly lockLabel = computed(() => {
    const lock = this.row()?.lock;
    return lock?.isMine
      ? 'You are editing this row'
      : `Being edited by ${lock?.userName ?? 'someone'}`;
  });

  public readonly caption = computed(() => this.layout().cell.caption ?? '');

  /** The caption doubles as the hint inside an empty control, marked if the cell is required. */
  public readonly hint = computed(() =>
    this.layout().cell.isRequired && this.caption() ? `${this.caption()} *` : this.caption(),
  );

  /** What a cell shows when it isn't a control. */
  public readonly display = computed(() => {
    const cell = this.cell();
    const cellType = this.cellType();
    const configuration = this.configuration();
    if (cell === null || cellType === null || configuration === null) {
      return '';
    }
    switch (cellType.kind) {
      case 'Text': {
        return cell.textValue ?? '';
      }
      case 'Number': {
        return this.formatNumber(
          cell.numberValue,
          'decimalPlaces' in configuration ? configuration.decimalPlaces : null,
          'unit' in configuration ? configuration.unit : null,
        );
      }
      case 'Date': {
        return cell.dateValue ?? '';
      }
      case 'TextDropdown':
      case 'NumberDropdown': {
        return cellType.options.find((option) => option.id === cell.optionId)?.value ?? '';
      }
      default: {
        return '';
      }
    }
  });

  public readonly isCheckbox = computed(() => this.cellType()?.kind === 'Checkbox');

  public readonly isChecked = computed(() => this.cell()?.booleanValue === true);

  private readonly store = inject(SheetStore);

  /** The grid placement plus the cell's background, which fills the whole grid area. */
  public hostStyle(): GridStyle {
    const background = this.styleCss()['background-color'];
    return background
      ? { ...this.layout().style, 'background-color': background }
      : this.layout().style;
  }

  /** The cell's text styling, with its alignment also lining up what's inside. */
  public contentStyle(): Record<string, string> {
    const { 'background-color': _background, ...text } = this.styleCss();
    const align = text['text-align'];
    return align ? { ...text, 'align-items': FLEX_ALIGNMENT[align] ?? 'flex-start' } : text;
  }

  public select(event: Event): void {
    event.stopPropagation();
    this.store.selectRow(this.layout().rowId);
  }

  /** The user moved into the cell's control: select its row. Nothing is locked or changed until they change a value. */
  public beginEditing(): void {
    this.store.selectRow(this.layout().rowId);
  }

  public save(request: CellValueRequest): void {
    const row = this.row();
    if (row !== null) {
      void this.store.saveValues(row.id, [request]);
    }
  }

  private formatNumber(
    value: number | null,
    decimals: number | null | undefined,
    unit: string | null | undefined,
  ): string {
    if (value === null) {
      return '';
    }
    const text =
      decimals === null || decimals === undefined ? String(value) : value.toFixed(decimals);
    return unit ? `${text} ${unit}` : text;
  }
}
