import { computed, linkedSignal } from '@angular/core';
import { CellValueHelpers } from '../../../../core/helpers/cell-value-helpers';
import { UpdateHelpers } from '../../../../core/helpers/update-helpers';
import { SpecSheetTableCell } from '../../../../core/models/domain/spec-sheet/spec-sheet-table-cell';
import { PairValue } from '../../../../core/models/domain/values/pair-value';
import { SpecSheetOp } from '../../../../core/models/operations/spec-sheet-op';
import { CreateSpecSheetTableCellRequest } from '../../../../core/models/requests/spec-sheets/cells/create-spec-sheet-table-cell-request';
import { UpdateSpecSheetTableCellRequest } from '../../../../core/models/requests/spec-sheets/cells/update-spec-sheet-table-cell-request';
import { ResetOption } from '../../types/cell-reset-options';
import { PUSpecSheetTableRow } from '../pu-spec-sheet-table-row';
import { PUSpecSheetTableCellBase } from './pu-spec-sheet-table-cell-base';

export class PUSpecSheetTableCellPair extends PUSpecSheetTableCellBase {
  public override readonly value = linkedSignal(() => {
    const cellValue = this.cellValue as PairValue;
    if (!cellValue) {
      return undefined;
    }

    return cellValue.valueX;
  });
  public readonly valueY = linkedSignal(() => {
    const cellValue = this.cellValue as PairValue;
    if (!cellValue) {
      return undefined;
    }

    return cellValue.valueY;
  });
  public override readonly displayValue = computed(
    () => `${this.value()} - ${this.valueY()}`,
  );

  public override readonly hasChanges = computed(() => {
    if (!this.cell) {
      return true;
    }

    return (
      this._changeTracker.hasChanges() ||
      this.value() !== this.originalValue ||
      this.valueY() !== this._originalValueY
    );
  });

  constructor(cell: SpecSheetTableCell, parent: PUSpecSheetTableRow) {
    super(cell, parent);
  }

  public override toTableCell(reset?: ResetOption): SpecSheetTableCell {
    let cellValue: any | undefined = undefined;
    switch (reset) {
      case 'all':
        cellValue = undefined;
        break;
      case 'none':
        cellValue = {
          valueX: this.value(),
          valueY: this.valueY(),
        };
        break;
    }

    return {
      id: reset ? 0 : this.cell.id,
      tableCellTypeId: this.cell.tableCellTypeId,
      specSheetTableRowId: this.cell.specSheetTableRowId,
      createdByUserId: this.cell?.createdByUserId,
      referenceId: reset ? crypto.randomUUID() : this.referenceId(),
      rowNumber: this.rowNumber(),
      columnNumber: this.columnNumber(),
      colSpan: this.colSpan(),
      rowSpan: this.rowSpan(),
      cellValue: CellValueHelpers.buildCellValue(this.cellValue, cellValue),
      createdDateTime: this.cell?.createdDateTime,
      isDeleted: this.cell?.isDeleted,
      cellConfigurationId: this.cell.cellConfigurationId,
      configurationOverride: this.cell.configurationOverride,
      cellConfiguration: this.cell?.cellConfiguration,
      tableCellType: this.tableCellType(),
    };
  }

  public override toNewTableCellRequest(): CreateSpecSheetTableCellRequest {
    return {
      specSheetTableGroupId: this.cell.specSheetTableRowId,
      specSheetTableRowId: this.cell.specSheetTableRowId,
      tableCellTypeId: this.cell.tableCellTypeId,
      cellConfigurationId: this.cell.cellConfigurationId,
      referenceId: this.referenceId(),
      rowNumber: this.rowNumber(),
      columnNumber: this.columnNumber(),
      colSpan: this.colSpan(),
      rowSpan: this.rowSpan(),
      value: CellValueHelpers.buildCellValue(this.cellValue, {
        valueX: this.value(),
        valueY: this.valueY(),
      }),
    };
  }

  public override toUpdateTableCellRequest(): UpdateSpecSheetTableCellRequest {
    return {
      id: this.id,
      rowNumber: UpdateHelpers.compare(this.cell.rowNumber, this.rowNumber()),
      columnNumber: UpdateHelpers.compare(
        this.cell.columnNumber,
        this.columnNumber(),
      ),
      cellValue: UpdateHelpers.compareToPropertyChanged(this.cellValue, {
        ...this.cellValue,
        valueX: this.value(),
        valueY: this.valueY(),
      } as PairValue),
    };
  }

  public override *compileValueOperations(): Generator<SpecSheetOp> {
    if (this.isNew) {
      yield {
        $type: 'AddPairValueOp',
        specSheetTableCellId: this.cell.id,
        valueX: this.value(),
        valueY: this.valueY(),
      };
    } else if (this.hasChanges()) {
      yield {
        $type: 'UpdatePairValueOp',
        id: this.cellValue.id,
        valueX: UpdateHelpers.compareToPropertyChanged(
          (this.cellValue as PairValue).valueX,
          this.value(),
        ),
        valueY: UpdateHelpers.compareToPropertyChanged(
          (this.cellValue as PairValue).valueY,
          this.valueY(),
        ),
      };
    }
  }

  public override get originalValue(): string | undefined {
    const cellValue = this.cellValue as PairValue;
    if (!cellValue) {
      return undefined;
    }

    return cellValue.valueX;
  }

  private get _originalValueY(): string | undefined {
    const cellValue = this.cellValue as PairValue;
    if (!cellValue) {
      return undefined;
    }

    return cellValue.valueY;
  }
}
