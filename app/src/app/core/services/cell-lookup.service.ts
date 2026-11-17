import { Injectable, signal } from '@angular/core';
import { PUSpecSheetTableCellBase } from '../../features/sheet/models/cells/pu-spec-sheet-table-cell-base';
import { PUSpecSheetTableRow } from '../../features/sheet/models/pu-spec-sheet-table-row';

@Injectable()
export class CellLookupService<
  C extends PUSpecSheetTableCellBase,
  T,
  G,
  R extends PUSpecSheetTableRow,
> {
  public readonly registrationComplete = signal(false);
  public readonly changed = signal(0);

  private readonly _cells = new Map<string, C>();
  private readonly _groups = new Map<string, G>();
  private readonly _rows = new Map<string, R>();
  private readonly _tables = new Map<string, T>();

  public registerTable(id: string, table: T): void {
    this._tables.set(id, table);
    this.triggerChanged();
  }

  public registerGroup(id: string, group: G): void {
    this._groups.set(id, group);
    this.triggerChanged();
  }

  public registerRow(id: string, row: R): void {
    this._rows.set(id, row);
    this.triggerChanged();
  }

  public registerCell(id: string, cell: C): void {
    this._cells.set(id, cell);
    this.triggerChanged();
  }

  public getTable(id: string): T | undefined {
    return this._tables.get(id);
  }

  public getGroup(id: string): G | undefined {
    return this._groups.get(id);
  }

  public getRow(id: string): R | undefined {
    return this._rows.get(id);
  }

  public getCell(id: string): C | undefined {
    return this._cells.get(id);
  }

  public removeTable(id: string): void {
    if (this._tables.has(id)) {
      this._tables.delete(id);
      this.triggerChanged();
    }
  }

  public removeGroup(id: string): void {
    if (this._groups.has(id)) {
      this._groups.delete(id);
      this.triggerChanged();
    }
  }

  public removeRow(id: string): void {
    if (this._rows.has(id)) {
      const row = this._rows.get(id);
      if (row && typeof row.destroy === 'function') {
        row.destroy();
      }

      this._rows.delete(id);
      this.triggerChanged();
    }
  }

  public removeCell(id: string): void {
    if (this._cells.has(id)) {
      const cell = this._cells.get(id);
      if (cell && typeof cell.destroy === 'function') {
        cell.destroy();
      }

      this._cells.delete(id);
      this.triggerChanged();
    }
  }

  public completeRegistration(): void {
    this.registrationComplete.set(true);
  }

  private triggerChanged(): void {
    this.changed.update((value) => value + 1);
  }

  public reset(): void {
    for (const cell of this._cells.values()) {
      if (cell && typeof cell.destroy === 'function') {
        cell.destroy();
      }
    }

    for (const row of this._rows.values()) {
      if (row && typeof row.destroy === 'function') {
        row.destroy();
      }
    }

    this._tables.clear();
    this._groups.clear();
    this._rows.clear();
    this._cells.clear();
    this.registrationComplete.set(false);
    this.triggerChanged();
  }
}
