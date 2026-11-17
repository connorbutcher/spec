import { EffectRef, inject, Signal } from '@angular/core';
import { SpecSheetExternalReference } from '../../../../core/models/domain/references/spec-sheet-external-reference';
import { CellLookupService } from '../../../../core/services/cell-lookup.service';
import { PUSpecSheetTableCellBase } from '../cells/pu-spec-sheet-table-cell-base';
import { PUSpecSheetTableGroup } from '../pu-spec-sheet-table-group';
import { PUSpecSheetTableRow } from '../pu-spec-sheet-table-row';
import { PUSpecSheetTableBase } from '../tables/pu-spec-sheet-table-base';

export abstract class ConfigurablePropertyBase {
  public abstract readonly isValid: Signal<boolean>;
  public abstract readonly hasChanges: Signal<boolean>;
  public abstract readonly internalReferences: Signal<
    SpecSheetExternalReference[]
  >;

  protected readonly _cellLookupService = inject<
    CellLookupService<
      PUSpecSheetTableCellBase,
      PUSpecSheetTableBase,
      PUSpecSheetTableGroup,
      PUSpecSheetTableRow
    >
  >(
    CellLookupService<
      PUSpecSheetTableCellBase,
      PUSpecSheetTableBase,
      PUSpecSheetTableGroup,
      PUSpecSheetTableRow
    >,
  );
  protected readonly destroyRefs: EffectRef[] = [];

  public destroy(): void {
    this.destroyRefs.forEach((ref) => ref.destroy());
  }
}
