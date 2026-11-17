import { moveItemInArray } from '@angular/cdk/drag-drop';
import {
  computed,
  inject,
  Injector,
  runInInjectionContext,
  signal,
  Signal,
  WritableSignal,
} from '@angular/core';
import { WorkflowState } from '../../../core/models/domain/enums/workflow-state';
import { SpecSheet } from '../../../core/models/domain/spec-sheet/spec-sheet';
import { SpecSheetTable } from '../../../core/models/domain/spec-sheet/spec-sheet-table';
import { SpecSheetPhase } from '../../../core/models/domain/types/spec-sheet-phase';
import { SpecSheetType } from '../../../core/models/domain/types/spec-sheet-type';
import { SpecSheetChanges } from '../../../core/models/operations/spec-sheet-changes';
import { SpecSheetOp } from '../../../core/models/operations/spec-sheet-op';
import { CellLookupService } from '../../../core/services/cell-lookup.service';
import { ValidatorLevel } from '../../../core/types/configuration/validator-level.enum';
import { SpecSheetTableHelpers } from '../helpers/spec-sheet-table-helpers';
import { SpecSheetAdvisory } from './spec-sheet-advisory';
import { PUSpecSheetTableBase } from './tables/pu-spec-sheet-table-base';

export class PUSpecSheet {
  public readonly id: number;
  public readonly versionNumber: Signal<number>;
  public readonly isPublished: Signal<boolean>;
  public readonly hasApprovalWorkflowStarted: boolean;
  public readonly requiresApprovalWorkflow: boolean;
  public readonly specSheetType: Signal<SpecSheetType>;
  public readonly specSheetPhase: Signal<SpecSheetPhase>;
  public readonly tables: WritableSignal<PUSpecSheetTableBase[]>;

  public readonly isValid = computed(() => {
    const errorAdvisories = this.advisories().filter(
      (a) => a.level === ValidatorLevel.Error,
    );
    return errorAdvisories.length === 0;
  });
  public readonly hasChanges = computed(() => {
    const specSheet = this.specSheet;
    if (!specSheet) {
      return true;
    }

    const originalTables = this.specSheet.tables;
    const currentTables = this.tables();
    let hasDeletedTables = false;
    for (const originalTable of originalTables) {
      const exists = currentTables.find((t) => t.id === originalTable.id);
      if (!exists) {
        hasDeletedTables = true;
        break;
      }
    }

    return (
      this.tables().some((t) => t.hasChanges() || t.isNew) || hasDeletedTables
    );
  });
  public readonly advisories = computed<SpecSheetAdvisory[]>(() => {
    return this.tables().flatMap((t) => t.advisories());
  });
  public readonly specSheetAdvisories = computed<SpecSheetAdvisory[]>(() => {
    const advisories: SpecSheetAdvisory[] = [];

    // Add issue if any of the tables have the same title.
    const tableTitles = this.tables().map((t) => t.title());
    const duplicateTitles = tableTitles.filter(
      (title, index) => tableTitles.indexOf(title) !== index,
    );
    if (duplicateTitles.length > 0) {
      advisories.push({
        level: ValidatorLevel.Error,
        message: 'Table titles must be unique.',
        referenceId: '',
      });
    }

    return advisories;
  });

  private readonly _injector = inject(Injector);
  private readonly _cellLookupService = inject(CellLookupService);

  constructor(public readonly specSheet: SpecSheet) {
    this.id = specSheet.id;
    this.versionNumber = signal(specSheet.versionNumber);
    this.isPublished = signal(specSheet.isPublished);
    this.specSheetType = signal(specSheet.specSheetType);
    this.specSheetPhase = signal(specSheet.specSheetPhase);
    this.tables = signal(
      specSheet.tables.map((table) =>
        SpecSheetTableHelpers.toTable(table, this),
      ),
    );
    this.requiresApprovalWorkflow =
      specSheet.specSheetType.requiresApprovalWorkflow;

    if (specSheet.specSheetType.requiresApprovalWorkflow) {
      this.hasApprovalWorkflowStarted = specSheet.workflowSteps.some(
        (ws) => ws.state === WorkflowState.Pending,
      );
    } else {
      this.hasApprovalWorkflowStarted = true;
    }
  }

  public addTable(table: SpecSheetTable): void {
    runInInjectionContext(this._injector, () => {
      const puTable = SpecSheetTableHelpers.toTable(table, this);
      this.tables.update((tables) => [...tables, puTable]);
    });
  }

  public deleteTable(table: PUSpecSheetTableBase): void {
    this.tables.update((tables) => tables.filter((t) => t.id !== table.id));

    for (let i = 0; i < this.tables().length; i++) {
      const table = this.tables()[i];
      table.displayOrder.set(i + 1);
    }

    this._cellLookupService.removeTable(table.referenceId());
  }

  public moveTable(previousIndex: number, currentIndex: number): void {
    moveItemInArray(this.tables(), previousIndex, currentIndex);

    for (let i = 0; i < this.tables().length; i++) {
      const table = this.tables()[i];
      table.displayOrder.set(i + 1);
    }
  }

  public compileChanges(): SpecSheetChanges | undefined {
    const operations = Array.from(
      this.tables().flatMap((t) => [...t.compileOperations()]),
    );
    operations.push(...this.compileDeletedTableOperations());
    if (operations.length === 0) {
      return undefined;
    }

    return {
      specSheetId: this.id,
      operations: operations,
    };
  }

  private *compileDeletedTableOperations(): Generator<SpecSheetOp> {
    for (const originalTable of this.specSheet.tables) {
      const exists = this.tables().find((t) => t.id === originalTable.id);
      if (!exists) {
        yield {
          $type: 'DeleteSpecSheetTableOp',
          id: originalTable.id,
        };
      }
    }
  }
}
