import {
  Directive,
  ElementRef,
  HostListener,
  inject,
  input,
} from '@angular/core';
import { PUSpecSheetTableGroup } from '../models/pu-spec-sheet-table-group';
import { PUSpecSheetTableRow } from '../models/pu-spec-sheet-table-row';
import { PUSpecSheetTableBase } from '../models/tables/pu-spec-sheet-table-base';
import { ActiveSheetService } from '../services/active-sheet.service';
import { RowContextMenuService } from '../services/row-context-menu.service';

@Directive({
  selector: 'appRowHover',
  standalone: true,
})
export class RowHoverDirective {
  public readonly table = input.required<PUSpecSheetTableBase>();
  public readonly parentGroup = input.required<PUSpecSheetTableGroup>();
  public readonly group = input.required<PUSpecSheetTableGroup>();
  public readonly row = input<PUSpecSheetTableRow>();
  public readonly hoverEnabled = input<boolean>(true);

  private readonly _rowHoverService = inject(RowContextMenuService);
  private readonly _activeSheetService = inject(ActiveSheetService);
  private _clearTimeout: number | undefined;
  private static _lastClearTimeout: number | undefined;

  constructor(private readonly _elementRef: ElementRef<HTMLTableRowElement>) {}

  @HostListener('mouseenter')
  onMouseEnter(): void {
    if (!this._activeSheetService.canEdit() || !this.hoverEnabled()) {
      return;
    }

    // Cancel any pending clear from ANY row
    if (RowHoverDirective._lastClearTimeout) {
      clearTimeout(RowHoverDirective._lastClearTimeout);
      RowHoverDirective._lastClearTimeout = undefined;
    }

    this._rowHoverService.setActiveElement({
      element: this._elementRef.nativeElement,
      table: this.table(),
      parentGroup: this.parentGroup(),
      group: this.group(),
      row: this.row(),
    });
  }

  @HostListener('mouseleave')
  onMouseLeave(): void {
    this._clearTimeout = setTimeout(() => {
      if (!this._rowHoverService.isContextMenuHovered()) {
        this._rowHoverService.setActiveElement(undefined);
      }

      RowHoverDirective._lastClearTimeout = undefined;
    }, 150);

    RowHoverDirective._lastClearTimeout = this._clearTimeout;
  }
}
