import { computed, linkedSignal } from '@angular/core';
import { CellValueHelpers } from '../../../../core/helpers/cell-value-helpers';
import { UpdateHelpers } from '../../../../core/helpers/update-helpers';
import { PairValue } from '../../../../core/models/domain/values/pair-value';
import { SpecSheetOp } from '../../../../core/models/operations/spec-sheet-op';
import { ResetOption } from '../../types/cell-reset-options';
import { PUSpecSheetTableRow } from '../pu-spec-sheet-table-row';
import { PUSpecSheetTableCellBase } from './pu-spec-sheet-table-cell-base';
import { SheetCellValue } from '../../../../core/models/domain/sheets/sheet-cell-value';
import { SheetTableCell } from '../../../../core/models/domain/sheets/sheet-table-cell';

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

  constructor(cell: SheetTableCell, parent: PUSpecSheetTableRow) {
    super(cell, parent);
  }

  public override toTableCell(reset?: ResetOption): SheetTableCell {
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
      sheetTableRowId: this.cell.sheetTableRowId,
      referenceId: reset ? crypto.randomUUID() : this.referenceId(),
      columnNumber: this.columnNumber(),
      colSpan: this.colSpan(),
      rowSpan: this.rowSpan(),
      value: CellValueHelpers.buildCellValue(
        this.cellValue,
        cellValue,
      ) as SheetCellValue,
      tableCellTemplateId: this.cell.tableCellTemplateId,
      createdDateTime: this.cell?.createdDateTime,
      cellConfigurationId: this.cell.cellConfigurationId,
      configurationOverride: this.cell.configurationOverride,
      cellConfiguration: this.cell?.cellConfiguration,
      tableCellType: this.tableCellType(),
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
        id: this.cell.id,
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
