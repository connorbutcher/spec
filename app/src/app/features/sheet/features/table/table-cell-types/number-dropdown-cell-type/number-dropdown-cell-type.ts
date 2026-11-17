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
import { NumberDropdownCellProperty } from '../../../cell-properties/models/number-dropdown-cell-property';
import { CellTypeBase } from '../cell-type-base';

@Component({
  selector: 'app-number-dropdown-cell-type',
  imports: [FormsModule, A11yModule, Select, ButtonDirective],
  templateUrl: './number-dropdown-cell-type.html',
  styleUrl: './number-dropdown-cell-type.scss',
  host: { class: 'cell-host' },
})
export class NumberDropdownCellType extends CellTypeBase<
  number,
  PUSpecSheetTableCell,
  NumberDropdownCellProperty
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
  public readonly allOptions = computed<number[]>(() =>
    this.properties().options(),
  );

  private _pendingKeyboardFocus = false;

  private readonly _dialog = inject(Dialog);

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
