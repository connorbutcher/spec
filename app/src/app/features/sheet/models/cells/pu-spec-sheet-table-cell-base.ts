import {
  computed,
  EffectRef,
  inject,
  linkedSignal,
  signal,
  Signal,
} from '@angular/core';
import { CellValueHelpers } from '../../../../core/helpers/cell-value-helpers';
import { UpdateHelpers } from '../../../../core/helpers/update-helpers';
import { ValidatorProperty } from '../../../../core/models/domain/configuration/validator-properties/validator-property';
import { CellTypeReferenceKey } from '../../../../core/models/domain/enums/cell-type-reference-key';
import { CellProperties } from '../../../../core/models/domain/properties/cell/cell-properties';
import { TableCellType } from '../../../../core/models/domain/template/table-cell-type';
import { CellValue } from '../../../../core/models/domain/values/cell-value';
import { SpecSheetOp } from '../../../../core/models/operations/spec-sheet-op';
import { CellLookupService } from '../../../../core/services/cell-lookup.service';
import { ValidatorLevel } from '../../../../core/types/configuration/validator-level.enum';
import { CellPropertiesClassHelpers } from '../../features/cell-properties/helpers/cell-properties-class-helper';
import { CellPropertyBase } from '../../features/cell-properties/models/cell-property-base';
import { ResetOption } from '../../types/cell-reset-options';
import { PUSpecSheetTableRow } from '../pu-spec-sheet-table-row';
import { AdvisoryAggregator } from '../services/advisory-aggregator';
import { ChangeTracker } from '../services/change-tracker';
import { ValidatorRuleRegistry } from '../services/validator-rule-registry';
import { SpecSheetAdvisory } from '../spec-sheet-advisory';
import { ValidatorBase } from '../validators/validator-base';
import { StylingConfiguration } from './styling-configuration';
import { SheetCellValue, SheetExternalReference, SheetTableCell } from '../../../../core/models/domain/sheets/sheet';

export abstract class PUSpecSheetTableCellBase<
  CV extends CellValue = CellValue,
  P extends CellPropertyBase<CellProperties> = CellPropertyBase<CellProperties>,
> {
  public abstract get originalValue(): any;

  public readonly id: number;
  public readonly cellValue: CV;
  public readonly referenceId: Signal<string>;
  public readonly rowNumber: Signal<number>;
  public readonly columnNumber: Signal<number>;
  public readonly rowSpan: Signal<number>;
  public readonly colSpan: Signal<number>;
  public readonly stylingConfiguration: Signal<StylingConfiguration>;
  public readonly properties = signal<P | undefined>(undefined);
  public readonly isIdentifier: Signal<boolean>;
  public readonly tableCellType: Signal<TableCellType>;
  public readonly validators = signal<ValidatorBase<ValidatorProperty>[]>([]);
  public readonly value = linkedSignal(() => {
    const cellValue = this.cellValue;
    if (!cellValue) {
      return null;
    }

    return (cellValue as any).value;
  });
  public readonly displayValue = computed(() => this.value());
  public readonly isValid = computed(() => this.advisories().length === 0);
  public readonly hasChanges: Signal<boolean>;
  public readonly advisories = computed<SpecSheetAdvisory[]>(() => {
    const advisories: SpecSheetAdvisory[] = [];
    const validators = this.validators();
    for (const validator of validators) {
      if (!validator.isValid()) {
        advisories.push({
          level: validator.level,
          message: validator.message(),
          referenceId: this.referenceId(),
        });
      }
    }

    if (this.properties() && !this.properties()?.isValid()) {
      advisories.push({
        level: ValidatorLevel.Error,
        message: 'Cell properties are not valid.',
        referenceId: this.referenceId(),
      });
    }

    if (this.rowNumber() < 0) {
      advisories.push({
        level: ValidatorLevel.Error,
        message: 'Row number must be 0 or greater.',
        referenceId: this.referenceId(),
      });
    }

    if (this.columnNumber() < 0) {
      advisories.push({
        level: ValidatorLevel.Error,
        message: 'Column number must be 0 or greater.',
        referenceId: this.referenceId(),
      });
    }

    if (this.colSpan() <= 0) {
      advisories.push({
        level: ValidatorLevel.Error,
        message: 'Column span must be greater than 0.',
        referenceId: this.referenceId(),
      });
    }

    if (this.rowSpan() <= 0) {
      advisories.push({
        level: ValidatorLevel.Error,
        message: 'Row span must be greater than 0.',
        referenceId: this.referenceId(),
      });
    }

    return advisories;
  });
  public readonly advisoryState = AdvisoryAggregator.getWorstState(
    this.advisories,
  );
  public readonly internalReferences = computed(() => {
    const references: SheetExternalReference[] = [];

    const properties = this.properties();
    if (properties && properties.internalReferences()) {
      references.push(...properties.internalReferences());
    }

    return references;
  });
  protected _changeTracker!: ChangeTracker<SheetTableCell>;
  protected readonly originalRowNumber: number;

  protected readonly cellLookupService = inject(CellLookupService);
  protected readonly destroyRefs: EffectRef[] = [];

  constructor(
    public readonly cell: SheetTableCell,
    private readonly _parent: PUSpecSheetTableRow,
  ) {
    this.id = cell.id;
    this.cellValue = cell.value as CV;
    this.referenceId = signal(cell.referenceId);
    this.originalRowNumber = _parent.row.displayOrder;
    this.rowNumber = signal(this.originalRowNumber);
    this.columnNumber = signal(cell.columnNumber);
    this.rowSpan = signal(cell.rowSpan);
    this.colSpan = signal(cell.colSpan);
    this.isIdentifier = signal(cell.cellConfiguration!.isIdentifier);
    this.tableCellType = signal(cell.tableCellType);

    this.stylingConfiguration = signal(
      new StylingConfiguration(this.tableCellType(), cell.cellConfiguration!),
    );

    this.properties.set(
      CellPropertiesClassHelpers.mapToClassFromCell(
        this.tableCellType(),
        this.cell.configurationOverride
          ? this.cell.configurationOverride
          : this.cell.cellConfiguration!.properties,
      ) as P,
    );

    if (cell.cellConfiguration && cell.cellConfiguration.validators) {
      const validators: ValidatorBase<any>[] = [];
      for (const validator of cell.cellConfiguration.validators) {
        const validatorInstance = ValidatorRuleRegistry.createValidator(
          validator.type,
          validator,
          [this],
        );
        if (validatorInstance) {
          validators.push(validatorInstance);
        }
      }

      this.validators.set(validators);
    }

    this._changeTracker = new ChangeTracker(cell)
      .track('columnNumber', this.columnNumber)
      .track('colSpan', this.colSpan)
      .track('rowSpan', this.rowSpan);
    this.hasChanges = computed(() => {
      const hasChanges =
        this._changeTracker.hasChanges() ||
        this.properties()?.hasChanges() ||
        !UpdateHelpers.isEqual(this.originalValue, this.value());

      return hasChanges;
    });

    this.cellLookupService.registerCell(this.referenceId(), this);
  }

  public remove(): void {
    this.cellLookupService.removeCell(this.referenceId());
  }

  public toTableCell(reset?: ResetOption): SheetTableCell {
    let cellValue: any | undefined = undefined;
    switch (reset) {
      case 'all':
        cellValue =
          this.tableCellType()?.key! === CellTypeReferenceKey.Description ||
          this.tableCellType()?.key! === CellTypeReferenceKey.Header
            ? this.value()
            : undefined;
        break;
      case 'none':
        cellValue = this.value();
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
      value: CellValueHelpers.buildCellValue(this.cellValue, cellValue) as SheetCellValue,
      tableCellTemplateId: this.cell.tableCellTemplateId,
      createdDateTime: this.cell?.createdDateTime,
      cellConfiguration: this.cell?.cellConfiguration,
      tableCellType: this.tableCellType(),
      configurationOverride: this.cell.configurationOverride,
    };
  }



  public *compileOperations(): Generator<SpecSheetOp> {
    if (this.isNew) {
      yield {
        $type: 'AddSpecSheetTableCellOp',
        tempId: this.id,
        specSheetTableRowId: this._parent.id,
        tableCellTypeId: this.cell.tableCellTypeId,
        cellConfigurationId: this.cell.cellConfigurationId,
        referenceId: this.referenceId(),
        rowNumber: this.rowNumber(),
        columnNumber: this.columnNumber(),
        colSpan: this.colSpan(),
        rowSpan: this.rowSpan(),
        configurationOverride: this.properties()?.toCellConfiguration(),
      };
    } else if (
      this._changeTracker.hasChanges() ||
      this.properties()?.hasChanges()
    ) {
      yield {
        $type: 'UpdateSpecSheetTableCellOp',
        id: this.id,
        rowNumber: UpdateHelpers.compare(this.originalRowNumber, this.rowNumber()),
        columnNumber: UpdateHelpers.compare(
          this.cell.columnNumber,
          this.columnNumber(),
        ),
        configurationOverride: UpdateHelpers.compareToPropertyChanged(
          this.cell.configurationOverride,
          this.properties()?.toCellConfiguration(),
        ),
      };
    }

    yield* this.compileValueOperations();
  }

  public *compileValueOperations(): Generator<SpecSheetOp> {
    if (this.isNew) {
      if (
        this.tableCellType().modelType === 'numeric' ||
        this.tableCellType().modelType === 'numericDropdown'
      ) {
        yield {
          $type: 'AddNumberValueOp',
          specSheetTableCellId: this.cell.id,
          value: this.value(),
        };
      } else {
        yield {
          $type: 'AddTextValueOp',
          specSheetTableCellId: this.cell.id,
          value: this.value(),
        };
      }
    } else if (!UpdateHelpers.isEqual(this.originalValue, this.value())) {
      if (
        this.tableCellType().modelType === 'numeric' ||
        this.tableCellType().modelType === 'numericDropdown'
      ) {
        yield {
          $type: 'UpdateNumberValueOp',
          id: this.cell.id,
          value: UpdateHelpers.compareToPropertyChanged(
            (this.cellValue as any).value,
            this.value(),
          ),
        };
      } else {
        yield {
          $type: 'UpdateTextValueOp',
          id: this.cell.id,
          value: UpdateHelpers.compareToPropertyChanged(
            (this.cellValue as any).value,
            this.value(),
          ),
        };
      }
    }
  }

  public destroy(): void {
    for (const ref of this.destroyRefs) {
      ref.destroy();
    }

    this.properties()?.destroy();
  }

  public get isNew(): boolean {
    return this.id <= 0;
  }
}
