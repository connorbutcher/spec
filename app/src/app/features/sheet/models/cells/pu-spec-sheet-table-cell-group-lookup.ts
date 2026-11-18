import { computed, effect, linkedSignal } from '@angular/core';
import { CellValueHelpers } from '../../../../core/helpers/cell-value-helpers';
import { UpdateHelpers } from '../../../../core/helpers/update-helpers';
import { LinkedSpecSheetValue } from '../../../../core/models/domain/values/linked-spec-sheet-value';
import { SpecSheetOp } from '../../../../core/models/operations/spec-sheet-op';
import { TableValuesDropdownCellProperty } from '../../features/cell-properties/models/table-values-dropdown-cell-property';
import { ResetOption } from '../../types/cell-reset-options';
import { PUSpecSheetTableRow } from '../pu-spec-sheet-table-row';
import { PUSpecSheetTableCellBase } from './pu-spec-sheet-table-cell-base';
import {
  SheetCellValue,
  SheetTableCell,
} from '../../../../core/models/domain/sheets/sheet';

export class PUSpecSheetTableCellGroupLookup extends PUSpecSheetTableCellBase<
  LinkedSpecSheetValue,
  TableValuesDropdownCellProperty
> {
  public override readonly value = linkedSignal<
    PUSpecSheetTableCellBase | undefined
  >(() => {
    // Read every dependency before any early return so the computation stays reactive.
    this.cellLookupService.changed();
    this.cellLookupService.registrationComplete();
    this.properties()?.table();

    const referenceId = this.cellValue?.linkedReferenceId;
    if (!referenceId) {
      return undefined;
    }

    return this.cellLookupService.getCell(referenceId);
  });
  public override readonly displayValue = computed(() =>
    this.value()?.displayValue(),
  );
  public override readonly hasChanges = computed(() => {
    if (!this.cell) {
      return true;
    }

    return (
      this._changeTracker.hasChanges() ||
      this.properties()?.hasChanges() ||
      this.originalValue !== this.value()?.referenceId()
    );
  });

  constructor(cell: SheetTableCell, parent: PUSpecSheetTableRow) {
    super(cell, parent);

    const ref = effect(() => {
      const propertiesChanged = this.properties()?.hasChanges();
      if (!propertiesChanged) {
        return;
      }

      this.value.set(undefined);
    });
    this.destroyRefs.push(ref);
  }

  public override toTableCell(reset?: ResetOption): SheetTableCell {
    let cellValue: any | undefined = undefined;
    switch (reset) {
      case 'all':
        cellValue = undefined;
        break;
      case 'none':
        cellValue = {
          ...this.properties()?.toCellConfiguration(),
          linkedReferenceId: this.value()?.referenceId(),
        } as LinkedSpecSheetValue;
        break;
    }

    return {
      id: reset ? 0 : this.cell.id,
      tableCellTypeId: this.cell.tableCellTypeId,
      sheetTableRowId: this.cell.sheetTableRowId,
      cellConfigurationId: this.cell.cellConfigurationId,
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
      cellConfiguration: this.cell?.cellConfiguration,
      tableCellType: this.tableCellType(),
      configurationOverride: this.cell.configurationOverride,
    };
  }

  public override *compileValueOperations(): Generator<SpecSheetOp> {
    if (this.isNew) {
      yield {
        $type: 'AddLinkedSpecSheetValueOp',
        specSheetTableCellId: this.cell.id,
        linkedSpecSheetTypeId: this.properties()?.specSheetType()?.id,
        linkedSpecSheetPhaseId: this.properties()?.specSheetPhase()?.id,
        tableReferenceId: this.properties()?.table()?.referenceId(),
        linkedReferenceId: this.value()?.referenceId(),
      };
    } else if (this.hasChanges()) {
      yield {
        $type: 'UpdateLinkedSpecSheetValueOp',
        id: this.cell.id,
        linkedSpecSheetTypeId: UpdateHelpers.compareToPropertyChanged(
          this.properties()?.specSheetType()?.id,
          this.cellValue.linkedSpecSheetTypeId,
        ),
        linkedSpecSheetPhaseId: UpdateHelpers.compareToPropertyChanged(
          this.properties()?.specSheetPhase()?.id,
          this.cellValue.linkedSpecSheetPhaseId,
        ),
        tableReferenceId: UpdateHelpers.compareToPropertyChanged(
          this.properties()?.table()?.referenceId(),
          this.cellValue.tableReferenceId,
        ),
        linkedReferenceId: UpdateHelpers.compareToPropertyChanged(
          this.value()?.referenceId(),
          this.cellValue.linkedReferenceId,
        ),
      };
    }
  }

  public override get originalValue(): string | undefined {
    return this.cellValue.linkedReferenceId;
  }
}
