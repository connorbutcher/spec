import { PUSpecSheet } from '../models/pu-spec-sheet';
import { LinkedPUSpecSheetTable } from '../models/tables/linked-pu-spec-sheet-table';
import { PUSpecSheetTable } from '../models/tables/pu-spec-sheet-table';
import { SheetTable } from '../../../core/models/domain/sheets/sheet';

export class SpecSheetTableHelpers {
  public static toTable(
    table: SheetTable,
    puSpecSheet: PUSpecSheet,
  ): PUSpecSheetTable {
    const type = table.configurationOverride?.$type;

    switch (type) {
      case 'linked':
        return new LinkedPUSpecSheetTable(table, puSpecSheet);
      default:
        return new PUSpecSheetTable(table, puSpecSheet);
    }
  }
}
