import { SpecSheetTable } from '../../../core/models/domain/spec-sheet/spec-sheet-table';
import { PUSpecSheet } from '../models/pu-spec-sheet';
import { LinkedPUSpecSheetTable } from '../models/tables/linked-pu-spec-sheet-table';
import { PUSpecSheetTable } from '../models/tables/pu-spec-sheet-table';

export class SpecSheetTableHelpers {
  public static toTable(
    table: SpecSheetTable,
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
