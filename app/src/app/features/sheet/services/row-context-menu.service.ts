import { Injectable, signal } from '@angular/core';
import { PUSpecSheetTableGroup } from '../models/pu-spec-sheet-table-group';
import { PUSpecSheetTableRow } from '../models/pu-spec-sheet-table-row';
import { PUSpecSheetTableBase } from '../models/tables/pu-spec-sheet-table-base';

export type ActiveElement = {
  element: HTMLTableRowElement;
  table: PUSpecSheetTableBase;
  parentGroup: PUSpecSheetTableGroup;
  group: PUSpecSheetTableGroup;
  row?: PUSpecSheetTableRow;
};

@Injectable()
export class RowContextMenuService {
  public readonly activeElement = signal<ActiveElement | undefined>(undefined);
  public readonly isContextMenuHovered = signal<boolean>(false);

  public setActiveElement(activeElement: ActiveElement | undefined): void {
    this.activeElement.set(activeElement);
  }

  public setContextMenuHovered(hovered: boolean): void {
    this.isContextMenuHovered.set(hovered);
  }
}
