import { computed, effect, inject, linkedSignal, signal } from '@angular/core';
import { isNil } from 'lodash';
import { environment } from '../../../../../../../environments/environment';
import { SpecSheetPhaseHelpers } from '../../../../../core/helpers/spec-sheet-phase-helpers';
import { CellProperties } from '../../../../../core/models/domain/properties/cell/cell-properties';
import { LinkedTableProperties } from '../../../../../core/models/domain/properties/table/linked-table-properties';
import { TableTemplate } from '../../../../../core/models/domain/template/table-template';
import { SpecSheetPhase } from '../../../../../core/models/domain/types/spec-sheet-phase';
import { SpecSheetType } from '../../../../../core/models/domain/types/spec-sheet-type';
import { PhasesService } from '../../../../../core/services/phases.service';
import { TypesService } from '../../../../../core/services/types.service';
import { UserService } from '../../../../../core/services/user.service';
import { PUSpecSheet } from '../../../models/pu-spec-sheet';
import { PUSpecSheetTableBase } from '../../../models/tables/pu-spec-sheet-table-base';
import { LinkedSheetService } from '../../../services/linked-sheet.service';
import { TablePropertyBase } from './table-property-base';
import { SheetExternalReference } from '../../../../../core/models/domain/sheets/sheet-external-reference';

export class LinkedTableProperty extends TablePropertyBase<LinkedTableProperties> {
  public override readonly isValid = computed(
    () =>
      !!this.specSheetType() &&
      !!this.specSheetPhase() &&
      !!this.linkedSpecSheet() &&
      !!this.table() &&
      this.lockToLatestPublished() !== undefined,
  );
  public override readonly hasChanges = computed(
    () =>
      this.specSheetType()?.id !== this.properties.specSheetTypeId ||
      this.specSheetPhase()?.id !== this.properties.specSheetPhaseId ||
      this.table()?.referenceId() !== this.properties.tableReferenceId ||
      this.lockToLatestPublished() !== this.properties.lockToLatestPublished,
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
      sheetId: this._sheetService.specSheet()?.id!,
      externalId: this._sheetService.specSheet()?.id!,
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
      const specSheetTypeId = this.properties.specSheetTypeId;
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
      const specSheetPhaseId = this.properties.specSheetPhaseId;
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
  private readonly _phasesService = inject(PhasesService);
  private readonly _typesService = inject(TypesService);
  private readonly _userService = inject(UserService);

  constructor(tableTemplate: TableTemplate, properties: LinkedTableProperties) {
    super(tableTemplate, properties);

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

  public override set(value: LinkedTableProperties): void {
    this.specSheetType.set(
      this._typesService
        .specSheetTypes()
        .find((t) => t.id === value.specSheetTypeId),
    );
    this.specSheetPhase.set(
      SpecSheetPhaseHelpers.getPhaseByIdFromArray(
        this._phasesService.specSheetPhases(),
        value.specSheetPhaseId!,
      ),
    );
    this.linkedSpecSheet.set(
      this._linkedSheetService.getLinkedSpecSheet(
        value.specSheetTypeId!,
        value.specSheetPhaseId!,
      ),
    );
    this.table.set(
      this.linkedSpecSheet()
        ?.tables()
        .find((t) => t.referenceId() === value.tableReferenceId),
    );
    this.lockToLatestPublished.set(value.lockToLatestPublished || false);
  }

  public override toTemplate(): TableTemplate {
    return {
      ...this.tableTemplate!,
      defaultConfiguration: {
        ...this.tableTemplate!.defaultConfiguration!,
        ...this.toConfiguration(),
      },
    };
  }

  public override toConfiguration(): CellProperties {
    return {
      ...this.properties,
      specSheetTypeId: this.specSheetType()?.id,
      specSheetPhaseId: this.specSheetPhase()?.id,
      tableReferenceId: this.table()?.referenceId(),
      lockToLatestPublished: this.lockToLatestPublished(),
    } as LinkedTableProperties;
  }

  public override destroy(): void {
    for (const destroyRef of this.destroyRefs) {
      destroyRef.destroy();
    }
  }
}
