import { computed, effect, inject, linkedSignal, signal } from '@angular/core';
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
import { PUSpecSheet } from '../../../models/pu-spec-sheet';
import { PUSpecSheetTableBase } from '../../../models/tables/pu-spec-sheet-table-base';
import { LinkedSheetService } from '../../../services/linked-sheet.service';
import { SheetContext } from '../../../services/sheet-context';
import { CellPropertyBase } from './cell-property-base';
import { SheetExternalReference } from '../../../../../core/models/domain/sheets/sheet-external-reference';

export class LinkedSpecSheetValueDropdownPhaseTypeCellProperty extends CellPropertyBase<LinkedSpecSheetDropdownCellProperties> {
  public override readonly isValid = computed(
    () =>
      !!this.specSheetType() &&
      !!this.specSheetPhase() &&
      !!this.linkedSpecSheet() &&
      !!this.table(),
  );
  public override readonly hasChanges = computed(
    () =>
      this.specSheetType()?.id !== this.properties.linkedSpecSheetTypeId ||
      this.specSheetPhase()?.id !== this.properties.linkedSpecSheetPhaseId ||
      this.table()?.referenceId() !== this.properties.tableReferenceId,
    { equal: () => false },
  );
  public override readonly internalReferences = computed(() => {
    const references: SheetExternalReference[] = [];

    const type = this.specSheetType();
    const phase = this.specSheetPhase();
    const linkedSpecSheet = this.linkedSpecSheet();
    const table = this.table();
    if (!type || !phase || !linkedSpecSheet || !table) {
      return references;
    }

    references.push({
      id: -1,
      sheetId: this._activeSheetService.specSheet()?.id!,
      externalId: this._activeSheetService.specSheet()?.id!,
      referenceId: table.referenceId(),
      type: 'table',
      systemName: environment.appName,
      url: `${window.location.origin}/spec/${linkedSpecSheet.id}#${table.referenceId()}`,
      ownerName: this._userService.user()?.displayName || 'Unknown User',
      grouping: `${type.type} - ${phase.phase}`,
      description: `Linked table: ${table.title()}`,
      systemIconUrl: `${window.location.origin}/favicon.ico`,
    });

    return references;
  });

  public readonly defaultValue = linkedSignal<string | undefined>(() => {
    return this.properties.defaultValue;
  });
  public readonly table = linkedSignal<PUSpecSheetTableBase | undefined>(() => {
    const specSheet = this.linkedSpecSheet();
    const referenceId = this.properties.tableReferenceId;
    if (!specSheet || !referenceId) {
      return undefined;
    }

    return specSheet.tables().find((t) => t.referenceId() === referenceId);
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
  public readonly lockToLatestPublished = linkedSignal<boolean>(() => {
    if (isNil(this.properties.lockToLatestPublished)) {
      return false;
    }

    return this.properties.lockToLatestPublished;
  });

  public readonly linkedSpecSheet = signal<PUSpecSheet | undefined>(undefined);

  private readonly _linkedSheetService = inject(LinkedSheetService);
  private readonly _activeSheetService = inject(SheetContext);
  private readonly _phasesService = inject(PhasesService);
  private readonly _typesService = inject(TypesService);
  private readonly _userService = inject(UserService);

  constructor(
    tableCellType: TableCellType,
    properties: LinkedSpecSheetDropdownCellProperties,
    cellTemplate?: TableCellTemplate,
  ) {
    super(tableCellType, properties, cellTemplate);

    const ref = effect(async () => {
      const specSheetType = this.specSheetType();
      const specSheetPhase = this.specSheetPhase();
      const lockedToLatestPublished = this.lockToLatestPublished();
      if (!specSheetType || !specSheetPhase) {
        this.linkedSpecSheet.set(undefined);
        return;
      }

      if (
        this._linkedSheetService.hasLinkedSpecSheet(
          specSheetType.id,
          specSheetPhase.id,
        )
      ) {
        const existingPUSpecSheet = this._linkedSheetService.getLinkedSpecSheet(
          specSheetType.id,
          specSheetPhase.id,
        );
        this.linkedSpecSheet.set(existingPUSpecSheet);
      } else {
        const specSheet =
          await this._linkedSheetService.getLatestPublishedSpecSheetByTypeAndPhaseAsync(
            specSheetType.id,
            specSheetPhase.id,
            lockedToLatestPublished,
          );

        if (!specSheet) {
          this.linkedSpecSheet.set(undefined);
          return;
        }

        const puSpecSheet =
          this._linkedSheetService.setLinkedSpecSheet(specSheet);
        this.linkedSpecSheet.set(puSpecSheet);
      }
    });
    this.destroyRefs.push(ref);
  }

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

    this.linkedSpecSheet.set(
      this._linkedSheetService.getLinkedSpecSheet(
        value.linkedSpecSheetTypeId!,
        value.linkedSpecSheetPhaseId!,
      ),
    );

    this.table.set(
      this.linkedSpecSheet()
        ?.tables()
        .find((t) => t.referenceId() === value.tableReferenceId),
    );

    this.defaultValue.set(value.defaultValue);
    this.lockToLatestPublished.set(value.lockToLatestPublished || false);
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
      defaultValue: this.defaultValue(),
      lockToLatestPublished: this.lockToLatestPublished(),
    } as LinkedSpecSheetDropdownCellProperties;
  }

  public override destroy(): void {
    for (const destroyRef of this.destroyRefs) {
      destroyRef.destroy();
    }
  }
}
