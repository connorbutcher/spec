import { computed, inject, linkedSignal } from '@angular/core';
import { isNil } from 'lodash';
import { environment } from '../../../../../../../environments/environment';
import { SpecSheetPhaseHelpers } from '../../../../../core/helpers/spec-sheet-phase-helpers';
import { CellProperties } from '../../../../../core/models/domain/properties/cell/cell-properties';
import { LinkedSpecSheetDropdownCellProperties } from '../../../../../core/models/domain/properties/cell/linked-spec-sheet-dropdown-cell-properties';
import { TableCellTemplate } from '../../../../../core/models/domain/template/table-cell-template';
import { TableCellType } from '../../../../../core/models/domain/template/table-cell-type';
import { SpecSheetPhase } from '../../../../../core/models/domain/types/spec-sheet-phase';
import { SpecSheetType } from '../../../../../core/models/domain/types/spec-sheet-type';
import { PhasesService } from '../../../../../core/services/phases.service';
import { TypesService } from '../../../../../core/services/types.service';
import { UserService } from '../../../../../core/services/user.service';
import { PUSpecSheetTableRow } from '../../../models/pu-spec-sheet-table-row';
import { PUSpecSheetTableBase } from '../../../models/tables/pu-spec-sheet-table-base';
import { CellPropertyBase } from './cell-property-base';
import { SheetExternalReference } from '../../../../../core/models/domain/sheets/sheet-external-reference';

export class TableValuesDropdownCellProperty extends CellPropertyBase<LinkedSpecSheetDropdownCellProperties> {
  public override readonly isValid = computed(
    () =>
      !!this.table() &&
      !!this.specSheetType() &&
      !!this.specSheetPhase() &&
      !!this.row() &&
      !!this.cellType(),
  );
  public override readonly hasChanges = computed(
    () => this.table()?.referenceId() !== this.properties.tableReferenceId,
  );
  public override readonly internalReferences = computed(() => {
    const references: SheetExternalReference[] = [];

    const type = this.specSheetType();
    const phase = this.specSheetPhase();
    const table = this.table();
    if (!type || !phase || !table) {
      return references;
    }

    references.push({
      id: -1,
      sheetId: table.table.sheetId,
      externalId: table.table.sheetId,
      referenceId: table.referenceId(),
      type: 'table',
      systemName: environment.appName,
      url: `${window.location.origin}/spec/${table.table.sheetId}#${table.referenceId()}`,
      ownerName: this._userService.user()?.displayName || 'Unknown User',
      grouping: `${type.type} - ${phase.phase}`,
      description: `Linked table: ${table.title()}`,
      systemIconUrl: `${window.location.origin}/favicon.ico`,
    });

    return references;
  });

  public readonly specSheetType = linkedSignal<SpecSheetType | undefined>(
    () => {
      const specSheetTypeId = this.properties.linkedSpecSheetTypeId;
      if (isNil(specSheetTypeId)) {
        return undefined;
      }

      return this._typesService
        .specSheetTypes()
        .find((t) => t.id === specSheetTypeId);
    },
  );
  public readonly specSheetPhase = linkedSignal<SpecSheetPhase | undefined>(
    () => {
      const specSheetPhaseId = this.properties.linkedSpecSheetPhaseId;
      if (isNil(specSheetPhaseId)) {
        return undefined;
      }

      return SpecSheetPhaseHelpers.getPhaseByIdFromArray(
        this._phasesService.specSheetPhases(),
        specSheetPhaseId,
      );
    },
  );
  public readonly table = linkedSignal<PUSpecSheetTableBase | undefined>(() => {
    const completeRegistration = this._cellLookupService.registrationComplete();
    const referenceId = this.properties.tableReferenceId;
    if (!completeRegistration || !referenceId) {
      return undefined;
    }

    return this._cellLookupService.getTable(referenceId);
  });
  public readonly row = linkedSignal<PUSpecSheetTableRow | undefined>(() => {
    const completeRegistration = this._cellLookupService.registrationComplete();
    const referenceId = this.properties.rowReferenceId;
    if (!completeRegistration || !referenceId) {
      return undefined;
    }

    return this._cellLookupService.getRow(referenceId);
  });
  public readonly cellType = linkedSignal<TableCellType | undefined>(() => {
    const completeRegistration = this._cellLookupService.registrationComplete();
    const referenceId = this.properties.tableCellTypeKey;
    if (!completeRegistration || !referenceId) {
      return undefined;
    }

    return this._typesService
      .tableCellTypes()
      .find((c) => c.key === referenceId);
  });
  public readonly defaultValue = linkedSignal<string | undefined>(() => {
    return this.properties.defaultValue;
  });
  public readonly lockToLatestPublished = linkedSignal<boolean>(() => {
    if (isNil(this.properties.lockToLatestPublished)) {
      return false;
    }

    return this.properties.lockToLatestPublished;
  });

  private readonly _phasesService = inject(PhasesService);
  private readonly _typesService = inject(TypesService);
  private readonly _userService = inject(UserService);

  public override set(value: LinkedSpecSheetDropdownCellProperties): void {
    this.specSheetType.set(
      this._typesService
        .specSheetTypes()
        .find((t) => t.id === value.linkedSpecSheetTypeId),
    );

    this.specSheetPhase.set(
      SpecSheetPhaseHelpers.getPhaseByIdFromArray(
        this._phasesService.specSheetPhases(),
        value.linkedSpecSheetPhaseId!,
      ),
    );

    this.table.set(
      value.tableReferenceId
        ? this._cellLookupService.getTable(value.tableReferenceId)
        : undefined,
    );

    this.defaultValue.set(value.defaultValue);
    this.row.set(
      value.rowReferenceId
        ? this._cellLookupService.getRow(value.rowReferenceId)
        : undefined,
    );
    this.cellType.set(
      value.tableCellTypeKey
        ? this._typesService
            .tableCellTypes()
            .find((c) => c.key === value.tableCellTypeKey)
        : undefined,
    );
  }

  public override toCellTemplate(): TableCellTemplate {
    return {
      ...this.cellTemplate!,
      cellConfiguration: {
        ...this.cellTemplate!.cellConfiguration!,
        properties: this.toCellConfiguration(),
      },
    };
  }

  public override toCellConfiguration(): CellProperties {
    return {
      ...this.properties,
      linkedSpecSheetTypeId: this.specSheetType()?.id,
      linkedSpecSheetPhaseId: this.specSheetPhase()?.id,
      tableReferenceId: this.table()?.referenceId(),
      rowReferenceId: this.row()?.referenceId(),
      tableCellTypeKey: this.cellType()?.key,
      defaultValue: this.defaultValue(),
      lockToLatestPublished: this.lockToLatestPublished(),
    } as LinkedSpecSheetDropdownCellProperties;
  }
}
