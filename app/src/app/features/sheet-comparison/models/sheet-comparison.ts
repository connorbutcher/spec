import { uniq } from 'lodash';
import { PUSpecSheet } from '../../sheet/models/pu-spec-sheet';
import { SpecSheetState } from './spec-sheet-state';
import { TableComparison } from './table-comparison';

export class SheetComparison {
  public readonly tableComparisons: TableComparison[] = [];

  constructor(
    public readonly left?: PUSpecSheet,
    public readonly right?: PUSpecSheet,
  ) {
    this.buildComparisons();
  }

  public get states(): SpecSheetState[] {
    return uniq(this.tableComparisons.flatMap((g) => g.states));
  }

  private buildComparisons(): void {
    for (const leftTable of this.left?.tables() || []) {
      const rightTable = this.right
        ?.tables()
        .find((t) => t.referenceId() === leftTable.referenceId());
      if (rightTable) {
        this.tableComparisons.push(new TableComparison(leftTable, rightTable));
      } else {
        this.tableComparisons.push(new TableComparison(leftTable, undefined));
      }
    }

    for (const rightTable of this.right?.tables() || []) {
      const leftTable = this.left
        ?.tables()
        .find((t) => t.referenceId() === rightTable.referenceId());
      if (!leftTable) {
        this.tableComparisons.push(new TableComparison(undefined, rightTable));
      }
    }
  }
}
