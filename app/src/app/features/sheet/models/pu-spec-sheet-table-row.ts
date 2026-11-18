import {
  computed,
  EffectRef,
  inject,
  signal,
  Signal,
  WritableSignal,
} from '@angular/core';
import { AdvisoryStateHelpers } from '../../../core/helpers/advisory-state-helpers';
import { UpdateHelpers } from '../../../core/helpers/update-helpers';
import { ValidatorProperty } from '../../../core/models/domain/configuration/validator-properties/validator-property';
import { TableRowTemplate } from '../../../core/models/domain/template/table-row-template';
import { SpecSheetOp } from '../../../core/models/operations/spec-sheet-op';
import { CellLookupService } from '../../../core/services/cell-lookup.service';
import { SpecSheetCellHelpers } from '../helpers/spec-sheet-cell-helpers';
import { SheetContext } from '../services/sheet-context';
import { ResetOption } from '../types/cell-reset-options';
import { PUSpecSheetTableCellBase } from './cells/pu-spec-sheet-table-cell-base';
import { ConfigurableCellGroup } from './configuration/configurable-cell-group';
import { PUSpecSheetTableGroup } from './pu-spec-sheet-table-group';
import { AdvisoryAggregator } from './services/advisory-aggregator';
import { ChangeTracker } from './services/change-tracker';
import { ValidatorRuleRegistry } from './services/validator-rule-registry';
import { SpecSheetAdvisory } from './spec-sheet-advisory';
import { ValidatorBase } from './validators/validator-base';
import {
  SheetExternalReference,
  SheetTableRow,
} from '../../../core/models/domain/sheets/sheet';

export class PUSpecSheetTableRow {
  public readonly id: number;
  public readonly referenceId: Signal<string>;
  public readonly rowNumber: WritableSignal<number>;
  public readonly cells: WritableSignal<PUSpecSheetTableCellBase[]>;
  public readonly validators = computed<ValidatorBase<ValidatorProperty>[]>(
    () => {
      const tableRowTemplate = this.tableRowTemplate();
      if (tableRowTemplate && tableRowTemplate.validators) {
        const validators: ValidatorBase<any>[] = [];
        for (const validator of tableRowTemplate.validators) {
          const instance = ValidatorRuleRegistry.createValidator(
            validator.type,
            validator,
            this.cells(),
          );
          if (instance) {
            validators.push(instance);
          }
        }

        return validators;
      }

      return [];
    },
  );
  public readonly tableRowTemplate = computed<TableRowTemplate | undefined>(
    () => {
      const row = this.row;
      if (!row) {
        return undefined!;
      }

      return this._activeSheetService
        .tableRowTemplates()
        .find((t) => t.id === row.tableRowTemplateId);
    },
  );
  public readonly identifierCells = computed(() =>
    this.cells().filter((c) => c.isIdentifier()),
  );
  public readonly configurableCells = computed<ConfigurableCellGroup[]>(() =>
    this.cells()
      .filter((c) => c.tableCellType().requiresUserConfiguration)
      .map((cell) => ({
        uid: cell.referenceId(),
        rowNumber: this.rowNumber(),
        columnNumber: cell.columnNumber(),
        tableCellType: cell.tableCellType(),
        cells: [cell],
      })),
  );

  public readonly isValid = computed(() => this.advisories().length === 0);
  public readonly hasChanges!: Signal<boolean>;
  private _changeTracker!: ChangeTracker<SheetTableRow>;
  // Own advisories: validator results for this row only
  private readonly _ownAdvisories = computed<SpecSheetAdvisory[]>(() => {
    const advisories: SpecSheetAdvisory[] = [];
    for (const validator of this.validators()) {
      if (!validator.isValid()) {
        advisories.push({
          level: validator.level,
          message: validator.message(),
          referenceId: this.referenceId(),
        });
      }
    }
    return advisories;
  });
  private readonly _cellAdvisories = computed<SpecSheetAdvisory[]>(() =>
    this.cells().flatMap((c) => c.advisories()),
  );

  // Aggregated advisories: own + all children (cells)
  public readonly advisories = AdvisoryAggregator.aggregate(
    this._ownAdvisories,
    [this._cellAdvisories],
  );
  public readonly advisoryState = AdvisoryAggregator.getWorstState(
    this.advisories,
  );
  public readonly advisoryStateClass = computed(() =>
    AdvisoryStateHelpers.getAdvisoryStateClass(this.advisoryState()!),
  );
  public readonly rowReferences = computed(() => {
    const externalReferences = this._activeSheetService.externalReferences();
    return externalReferences.filter(
      (er) => er.referenceId === this.referenceId(),
    );
  });
  public readonly internalReferences = computed(() => {
    const references: SheetExternalReference[] = [];

    const cells = this.cells().flatMap((r) => r.internalReferences());
    references.push(...cells);
    return references;
  });

  private readonly _activeSheetService = inject(SheetContext);
  private readonly _cellLookupService = inject(CellLookupService);
  private readonly destroyRefs: EffectRef[] = [];

  constructor(
    public readonly row: SheetTableRow,
    public readonly parent: PUSpecSheetTableGroup,
  ) {
    this.id = row.id;
    this.referenceId = signal(row.referenceId);
    this.rowNumber = signal(row.displayOrder);
    this.cells = signal(
      row.cells.map((cell) => SpecSheetCellHelpers.toCell(cell, this)),
    );

    this._changeTracker = new ChangeTracker(row).track(
      'displayOrder',
      this.rowNumber,
    );
    this.hasChanges = computed(() => {
      const hasDeletedCells = this.row.cells.some(
        (oc) => !this.cells().find((c) => c.id === oc.id),
      );

      return (
        this._changeTracker.hasChanges() ||
        this.cells().some((c) => c.hasChanges() || c.isNew) ||
        hasDeletedCells
      );
    });

    this._cellLookupService.registerRow(this.referenceId(), this);
  }

  public remove(): void {
    for (const cell of this.cells()) {
      cell.remove();
    }

    this._cellLookupService.removeRow(this.referenceId());
  }

  public *compileOperations(): Generator<SpecSheetOp> {
    if (this.isNew) {
      yield {
        $type: 'AddSpecSheetTableRowOp',
        tempId: this.id,
        specSheetTableGroupId: this.parent.id,
        tableRowTemplateId: this.row.tableRowTemplateId,
        referenceId: this.referenceId(),
        rowNumber: this.rowNumber(),
      };
    } else if (this._changeTracker.hasChanges()) {
      yield {
        $type: 'UpdateSpecSheetTableRowOp',
        id: this.id,
        rowNumber: UpdateHelpers.compare(
          this.row.displayOrder,
          this.rowNumber(),
        ),
      };
    }

    for (const cell of this.cells()) {
      yield* cell.compileOperations();
    }

    yield* this.compileDeleteOperations();
  }

  private *compileDeleteOperations(): Generator<SpecSheetOp> {
    for (const originalCell of this.row.cells) {
      const exists = this.cells().find((c) => c.id === originalCell.id);
      if (!exists) {
        yield {
          $type: 'DeleteSpecSheetTableCellOp',
          id: originalCell.id,
        };
      }
    }
  }

  public destroy(): void {
    for (const ref of this.destroyRefs) {
      ref.destroy();
    }
  }

  public get isNew(): boolean {
    return this.row.id <= 0;
  }
}
