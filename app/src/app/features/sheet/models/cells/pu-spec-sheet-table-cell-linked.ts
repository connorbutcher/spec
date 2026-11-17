import { computed, effect, linkedSignal } from '@angular/core';
import { CellValueHelpers } from '../../../../core/helpers/cell-value-helpers';
import { UpdateHelpers } from '../../../../core/helpers/update-helpers';
import { SpecSheetTableCell } from '../../../../core/models/domain/spec-sheet/spec-sheet-table-cell';
import { LinkedSpecSheetValue } from '../../../../core/models/domain/values/linked-spec-sheet-value';
import { SpecSheetOp } from '../../../../core/models/operations/spec-sheet-op';
import { CreateSpecSheetTableCellRequest } from '../../../../core/models/requests/spec-sheets/cells/create-spec-sheet-table-cell-request';
import { UpdateSpecSheetTableCellRequest } from '../../../../core/models/requests/spec-sheets/cells/update-spec-sheet-table-cell-request';
import { LinkedSpecSheetValueDropdownPhaseTypeCellProperty } from '../../features/cell-properties/models/linked-spec-sheet-value-dropdown-phase-type-cell-property';
import { ResetOption } from '../../types/cell-reset-options';
import { PUSpecSheetTableRow } from '../pu-spec-sheet-table-row';
import { PUSpecSheetTableCellBase } from './pu-spec-sheet-table-cell-base';

export class PUSpecSheetTableCellLinked extends PUSpecSheetTableCellBase<
  LinkedSpecSheetValue,
  LinkedSpecSheetValueDropdownPhaseTypeCellProperty
> {
  public override readonly value =
    linkedSignal<PUSpecSheetTableCellBase | null>(() => {
      // Read every dependency before any early return so the computation stays reactive.
      this.cellLookupService.changed();
      this.cellLookupService.registrationComplete();
      this.properties();

      const referenceId = this.cellValue?.linkedReferenceId;
      if (!referenceId) {
        return null;
      }

      return this.cellLookupService.getCell(referenceId) ?? null;
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
      this.originalValue !== (this.value()?.referenceId() ?? null)
    );
  });

  constructor(cell: SpecSheetTableCell, parent: PUSpecSheetTableRow) {
    super(cell, parent);

    const ref = effect(() => {
      const propertiesChanged = this.properties()?.hasChanges();
      if (!propertiesChanged) {
        return;
      }

      this.value.set(null);
    });
    this.destroyRefs.push(ref);
  }

  public override toTableCell(reset?: ResetOption): SpecSheetTableCell {
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
      specSheetTableRowId: this.cell.specSheetTableRowId,
      cellConfigurationId: this.cell.cellConfigurationId,
      createdByUserId: this.cell?.createdByUserId,
      referenceId: reset ? crypto.randomUUID() : this.referenceId(),
      rowNumber: this.rowNumber(),
      columnNumber: this.columnNumber(),
      colSpan: this.colSpan(),
      rowSpan: this.rowSpan(),
      cellValue: CellValueHelpers.buildCellValue(this.cellValue, cellValue),
      createdDateTime: this.cell?.createdDateTime,
      isDeleted: this.cell?.isDeleted,
      cellConfiguration: this.cell?.cellConfiguration,
      tableCellType: this.tableCellType(),
      configurationOverride: this.cell.configurationOverride,
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
        ...this.properties()?.toCellConfiguration(),
        linkedReferenceId: this.value()?.referenceId(),
        linkedSpecSheetPhaseId: this.properties()?.specSheetPhase()?.id,
        linkedSpecSheetTypeId: this.properties()?.specSheetType()?.id,
        tableReferenceId: this.properties()?.table()?.referenceId(),
      }),
      configurationOverride: this.cell.configurationOverride,
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
      configurationOverride: UpdateHelpers.compareToPropertyChanged(
        this.cell.configurationOverride,
        this.properties()?.toCellConfiguration(),
      ),
      cellValue: UpdateHelpers.compareToPropertyChanged(this.cellValue, {
        $type: this.cellValue.$type,
        id: this.cellValue.id,
        specSheetTableCellId: this.cellValue.specSheetTableCellId,
        ...this.properties()?.toCellConfiguration(),
        linkedReferenceId: this.value()?.referenceId(),
        linkedSpecSheetPhaseId: this.properties()?.specSheetPhase()?.id,
        linkedSpecSheetTypeId: this.properties()?.specSheetType()?.id,
        tableReferenceId: this.properties()?.table()?.referenceId(),
      } as LinkedSpecSheetValue),
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
        id: this.cellValue.id,
        linkedSpecSheetTypeId: UpdateHelpers.compareToPropertyChanged(
          this.cellValue.linkedSpecSheetTypeId,
          this.properties()?.specSheetType()?.id,
        ),
        linkedSpecSheetPhaseId: UpdateHelpers.compareToPropertyChanged(
          this.cellValue.linkedSpecSheetPhaseId,
          this.properties()?.specSheetPhase()?.id,
        ),
        tableReferenceId: UpdateHelpers.compareToPropertyChanged(
          this.cellValue.tableReferenceId,
          this.properties()?.table()?.referenceId(),
        ),
        linkedReferenceId: UpdateHelpers.compareToPropertyChanged(
          this.cellValue.linkedReferenceId,
          this.value()?.referenceId(),
        ),
      };
    }
  }

  public override get originalValue(): any {
    return this.cellValue.linkedReferenceId;
  }
}
