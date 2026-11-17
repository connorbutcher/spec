import { computed, inject } from '@angular/core';
import { uniq } from 'lodash';
import { CellLookupService } from '../../../core/services/cell-lookup.service';
import { PUSpecSheetTableGroup } from '../../sheet/models/pu-spec-sheet-table-group';
import { SpecSheetState } from './spec-sheet-state';
import { TableRowComparison } from './table-row-comparison';

export class TableGroupComparison {
  public readonly rows: TableRowComparison[] = [];
  public readonly groups: TableGroupComparison[] = [];
  public readonly state: SpecSheetState = 'unchanged';
  public readonly columnDefinitions = computed(() => {
    const rows = this.rows;
    const childGroups = this.groups;

    let columnCount = 0;
    let definitionString = '';

    const columnNumbersSet = new Map<number, number>();
    for (const row of rows) {
      const rowValue = row.left ? row.left : row.right!;
      for (const cell of rowValue.cells()) {
        if (!columnNumbersSet.has(cell.columnNumber())) {
          columnNumbersSet.set(
            cell.columnNumber(),
            cell.stylingConfiguration().width || 10,
          );
        }
      }
    }

    columnCount = columnNumbersSet.size;
    definitionString = Array.from(columnNumbersSet.values())
      .map((width) => `${width}em`)
      .join(' ');

    for (const childGroup of childGroups) {
      columnNumbersSet.clear();
      for (const row of childGroup.rows) {
        const rowValue = row.left ? row.left : row.right!;
        for (const cell of rowValue.cells()) {
          if (!columnNumbersSet.has(cell.columnNumber())) {
            columnNumbersSet.set(
              cell.columnNumber(),
              cell.stylingConfiguration().width || 10,
            );
          }
        }
      }

      columnCount += columnNumbersSet.size;
      definitionString +=
        ' ' +
        Array.from(columnNumbersSet.values())
          .map((width) => `${width}em`)
          .join(' ');
    }

    return { count: columnCount, definitionString };
  });

  private readonly _cellLookupService = inject(CellLookupService);

  constructor(
    public readonly left: PUSpecSheetTableGroup | null,
    public readonly right: PUSpecSheetTableGroup | null,
  ) {
    if (!left && right) {
      this.state = 'added';
      this._cellLookupService.registerGroup(right.referenceId(), right);
    } else if (left && !right) {
      this.state = 'removed';
      this._cellLookupService.registerGroup(left.referenceId(), left);
    } else if (left && right && left.order !== right.order) {
      this.state = 'modified';
      this._cellLookupService.registerGroup(right.referenceId(), right);
    } else {
      this._cellLookupService.registerGroup(right!.referenceId(), right);
    }

    this.buildRowComparisons();
    this.buildChildGroupComparisons();
  }

  public get states(): SpecSheetState[] {
    return uniq([
      ...this.rows.flatMap((c) => c.states),
      ...this.groups.flatMap((g) => g.states),
    ]);
  }

  private buildRowComparisons(): void {
    for (const leftRow of this.left?.rows() || []) {
      const matchingRightRow = this.right
        ?.rows()
        .find((c) => c.referenceId() === leftRow.referenceId());
      this.rows.push(new TableRowComparison(leftRow, matchingRightRow || null));
    }

    for (const rightRow of this.right?.rows() || []) {
      const hasMatchingLeftRow = this.rows.some(
        (c) =>
          c.left !== null && c.left.referenceId() === rightRow.referenceId(),
      );
      if (!hasMatchingLeftRow) {
        this.rows.push(new TableRowComparison(null, rightRow));
      }
    }
  }

  private buildChildGroupComparisons(): void {
    for (const leftGroup of this.left?.childGroups() || []) {
      const matchingRightGroup = this.right
        ?.childGroups()
        .find((r) => r.referenceId() === leftGroup.referenceId());
      this.groups.push(
        new TableGroupComparison(leftGroup, matchingRightGroup || null),
      );
    }

    for (const rightGroup of this.right?.childGroups() || []) {
      const hasMatchingLeftGroup = this.groups.some(
        (r) =>
          r.left !== null && r.left.referenceId() === rightGroup.referenceId(),
      );
      if (!hasMatchingLeftGroup) {
        this.groups.push(new TableGroupComparison(null, rightGroup));
      }
    }
  }
}
