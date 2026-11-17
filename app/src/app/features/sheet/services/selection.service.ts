import { Injectable, signal } from '@angular/core';
import { PUSpecSheetTableCellBase } from '../models/cells/pu-spec-sheet-table-cell-base';

@Injectable()
export class SelectionService {
  public readonly selectedCells = signal<PUSpecSheetTableCellBase[]>([]);

  public reset(): void {
    this.selectedCells.set([]);
  }

  public select(...cells: PUSpecSheetTableCellBase[]): void {
    this.selectedCells.update((current) => {
      const newCells = [...current, ...cells];
      const sortedSelection = newCells.sort((a, b) => {
        const columnDiff = a.columnNumber() - b.columnNumber();
        return columnDiff !== 0 ? columnDiff : a.rowNumber() - b.rowNumber();
      });

      return [...new Set(sortedSelection)];
    });
  }

  public isSelected(cell: PUSpecSheetTableCellBase): boolean {
    return this.selectedCells().includes(cell);
  }

  public unselect(...cells: PUSpecSheetTableCellBase[]): void {
    this.selectedCells.update((current) => {
      const newCells = current.filter((cell) => !cells.includes(cell));
      return newCells;
    });
  }
}
