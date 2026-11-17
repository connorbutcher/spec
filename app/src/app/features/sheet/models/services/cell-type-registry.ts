import { SpecSheetTableCell } from '../../../../core/models/domain/spec-sheet/spec-sheet-table-cell';
import { PUSpecSheetTableCell } from '../cells/pu-spec-sheet-table-cell';
import { PUSpecSheetTableCellBase } from '../cells/pu-spec-sheet-table-cell-base';
import { PUSpecSheetTableCellDropdown } from '../cells/pu-spec-sheet-table-cell-dropdown';
import { PUSpecSheetTableCellGroupLookup } from '../cells/pu-spec-sheet-table-cell-group-lookup';
import { PUSpecSheetTableCellLinked } from '../cells/pu-spec-sheet-table-cell-linked';
import { PUSpecSheetTableCellPair } from '../cells/pu-spec-sheet-table-cell-pair';
import { PUSpecSheetTableRow } from '../pu-spec-sheet-table-row';

export type CellFactory = (
  cell: SpecSheetTableCell,
  parent: PUSpecSheetTableRow,
) => PUSpecSheetTableCellBase;

export class CellTypeRegistry {
  private static readonly _registry = new Map<string, CellFactory>();

  static {
    CellTypeRegistry.registerDefaults();
  }

  public static register(type: string, factory: CellFactory): void {
    this._registry.set(type, factory);
  }

  public static createCell(
    cellType: string,
    cell: SpecSheetTableCell,
    parent: PUSpecSheetTableRow,
  ): PUSpecSheetTableCellBase | undefined {
    const factory = this._registry.get(cellType);
    if (!factory) {
      return undefined;
    }

    return factory(cell, parent);
  }

  private static registerDefaults(): void {
    // Standard cell types
    this.register(
      'text',
      (cell, parent) => new PUSpecSheetTableCell(cell, parent),
    );
    this.register(
      'numeric',
      (cell, parent) => new PUSpecSheetTableCell(cell, parent),
    );
    this.register(
      'default',
      (cell, parent) => new PUSpecSheetTableCell(cell, parent),
    );

    // Advanced cell types
    this.register(
      'textDropdown',
      (cell, parent) => new PUSpecSheetTableCellDropdown(cell, parent),
    );
    this.register(
      'numericDropdown',
      (cell, parent) => new PUSpecSheetTableCellDropdown(cell, parent),
    );
    this.register(
      'linkedSpecSheet',
      (cell, parent) => new PUSpecSheetTableCellLinked(cell, parent),
    );
    this.register(
      'pair',
      (cell, parent) => new PUSpecSheetTableCellPair(cell, parent),
    );
    this.register(
      'groupLookup',
      (cell, parent) => new PUSpecSheetTableCellGroupLookup(cell, parent),
    );
  }
}
