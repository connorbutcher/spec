import { A11yModule } from '@angular/cdk/a11y';
import { Dialog } from '@angular/cdk/dialog';
import {
  Component,
  computed,
  HostListener,
  inject,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ButtonDirective } from 'primeng/button';
import { Select } from 'primeng/select';
import { filter, skip, takeUntil } from 'rxjs';
import { ManageTableDialog } from '../../../../dialogs/manage-table-dialog/manage-table-dialog';
import { ManageTableDialogData } from '../../../../dialogs/manage-table-dialog/models/manage-table-dialog-data';
import { PUSpecSheetTableCell } from '../../../../models/cells/pu-spec-sheet-table-cell';
import { LinkedSpecSheetValueDropdownPhaseTypeCellProperty } from '../../../cell-properties/models/linked-spec-sheet-value-dropdown-phase-type-cell-property';
import { CellTypeBase } from '../cell-type-base';

export type DropdownOption = {
  value: PUSpecSheetTableCell;
  displayValue: string;
};

@Component({
  selector: 'app-linked-spec-sheet-dropdown-cell-type',
  imports: [FormsModule, A11yModule, Select, ButtonDirective],
  templateUrl: './linked-spec-sheet-dropdown-cell-type.html',
  styleUrl: './linked-spec-sheet-dropdown-cell-type.scss',
  host: { class: 'cell-host' },
})
export class LinkedSpecSheetDropdownCellType extends CellTypeBase<
  number,
  PUSpecSheetTableCell,
  LinkedSpecSheetValueDropdownPhaseTypeCellProperty
> {
  @HostListener('document:keydown', ['$event'])
  onGlobalKeydown(event: KeyboardEvent): void {
    if (event.key === 'Tab') {
      this._pendingKeyboardFocus = true;
    }
  }

  @HostListener('document:mouseup')
  @HostListener('document:mousedown')
  onGlobalPointer(): void {
    this._pendingKeyboardFocus = false;
  }

  public readonly dropdown = viewChild<Select>(Select);
  public readonly allOptions = computed<DropdownOption[]>(() => {
    const properties = this.properties();
    if (!properties) {
      return [];
    }

    const table = properties.table();
    if (!table) {
      return [];
    }

    return table
      .identifierCells()
      .map((c) => ({ value: c, displayValue: c.displayValue() }));
  });

  private readonly _dialog = inject(Dialog);

  private _pendingKeyboardFocus = false;

  constructor() {
    super();

    toObservable(this.cellValue)
      .pipe(skip(1), takeUntilDestroyed())
      .subscribe((value) => {
        this.cell().value.set(value);
      });
  }

  ngOnInit(): void {
    toObservable(this.cell().value, { injector: this._injector })
      .pipe(
        takeUntil(this.destroy),
        filter((value) => value !== this.cellValue()),
      )
      .subscribe(() => {
        this.cellValue.set(this.cell().value());
      });
  }

  public viewConfiguration(): void {
    this._dialog.open<unknown, ManageTableDialogData, ManageTableDialog>(
      ManageTableDialog,
      {
        data: { table: this.table(), cellToConfigure: this.cell() },
        injector: this._injector,
        minWidth: '40vw',
      },
    );
  }

  public override isMatch(cellValue: any, filter: string): boolean {
    if (cellValue === null || cellValue === undefined) {
      return false;
    }

    const cell = cellValue as PUSpecSheetTableCell;
    return cell.displayValue().toLocaleLowerCase().includes(filter);
  }

  public override focusCell(event: any): void {
    if (!this.canEdit()) {
      return;
    }

    if (!event) {
      this.isCellFocused.set(false);
      return;
    }

    this.isCellFocused.set(true);

    const dropdown = this.dropdown();
    if (!dropdown || !this.isCellFocused()) {
      return;
    }

    // Open only if focus arrived via Tab (flag set by global keydown)
    if (this._pendingKeyboardFocus && !dropdown.overlayVisible) {
      dropdown.show();
    }

    this._pendingKeyboardFocus = false; // reset after use
  }

  public onKeydown(event: KeyboardEvent): void {
    // Allow Enter / Space to toggle when already focused
    if ((event.key === 'Enter' || event.key === ' ') && this.isCellFocused()) {
      const dropdown = this.dropdown();
      if (dropdown && !dropdown.overlayVisible) dropdown.show();
      event.preventDefault();
    }
  }

  public onMousedown(): void {
    this._pendingKeyboardFocus = false;
  }
}
