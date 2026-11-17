import { inject } from '@angular/core';
import { uniq } from 'lodash';
import { CellLookupService } from '../../../core/services/cell-lookup.service';
import { PUSpecSheetTableBase } from '../../sheet/models/tables/pu-spec-sheet-table-base';
import { SpecSheetState } from './spec-sheet-state';
import { TableGroupComparison } from './table-group-comparison';

export class TableComparison {
  public readonly groups: TableGroupComparison[] = [];
  public readonly tableState: SpecSheetState = 'unchanged';
  public readonly referenceId: string;
  public readonly title: string;
  public readonly displayOrder: number;

  private readonly _cellLookupService = inject(CellLookupService);

  constructor(
    public readonly left?: PUSpecSheetTableBase,
    public readonly right?: PUSpecSheetTableBase,
  ) {
    if (!left && right) {
      this.tableState = 'added';

      this.referenceId = right.referenceId();
      this.title = right.title();
      this.displayOrder = right.displayOrder();

      this._cellLookupService.registerTable(right.referenceId(), right);
    } else if (left && !right) {
      this.tableState = 'removed';

      this.referenceId = left.referenceId();
      this.title = left.title();
      this.displayOrder = left.displayOrder();

      this._cellLookupService.registerTable(left.referenceId(), left);
    } else if (left && right && left.title() !== right.title()) {
      this.tableState = 'modified';

      this.referenceId = left.referenceId();
      this.title = `${left.title()} → ${right.title()}`;
      this.displayOrder = right.displayOrder();

      this._cellLookupService.registerTable(right.referenceId(), right);
    } else {
      this.referenceId = right!.referenceId();
      this.title = right!.title();
      this.displayOrder = right!.displayOrder();

      this._cellLookupService.registerTable(right!.referenceId(), right);
    }

    this.buildComparisons();
  }

  public get states(): SpecSheetState[] {
    return uniq(this.groups.flatMap((g) => g.states));
  }

  private buildComparisons(): void {
    if (this.left) {
      for (const leftGroup of this.left.groups()) {
        const matchingRightGroup = this.right
          ?.groups()
          .find((r) => r.referenceId() === leftGroup.referenceId());
        this.groups.push(
          new TableGroupComparison(leftGroup, matchingRightGroup || null),
        );
      }
    }

    if (this.right) {
      for (const rightGroup of this.right.groups()) {
        const hasMatchingLeftGroup = this.groups.some(
          (r) =>
            r.left !== null &&
            r.left.referenceId() === rightGroup.referenceId(),
        );
        if (!hasMatchingLeftGroup) {
          this.groups.push(new TableGroupComparison(null, rightGroup));
        }
      }
    }
  }
}
