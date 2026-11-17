import { Component, computed, effect, input, signal } from '@angular/core';
import { isNil } from 'lodash';
import { CellProperties } from '../../../../../core/models/domain/properties/cell/cell-properties';
import { Destroyable } from '../../../../common/destructable/destructable';
import { CellPropertyBase } from '../../../../sheet/features/cell-properties/models/cell-property-base';
import { PUSpecSheetTableCell } from '../../../../sheet/models/cells/pu-spec-sheet-table-cell';
import { PUSpecSheetTableCellBase } from '../../../../sheet/models/cells/pu-spec-sheet-table-cell-base';
import { TableCellComparison } from '../../../models/table-cell-comparison';
import { SheetComparisonFilterService } from '../../../services/sheet-comparison-filter.service';

@Component({
  selector: 'app-cell-type-base',
  template: '',
  standalone: true,
  host: {
    class: 'cell-host',
    '[style]': 'styling()',
    '[class.filtered-match]': 'isFilteredMatch()',
    '[class.filtered-active]': 'isFilteredActive()',
    '[class]': 'comparison().state',
  },
})
export class CellComparisonBase<
  C extends PUSpecSheetTableCellBase = PUSpecSheetTableCell,
  P extends CellPropertyBase<CellProperties> | undefined = undefined,
> extends Destroyable {
  public readonly comparison = input.required<TableCellComparison>();
  public readonly cell = input.required<C>();
  public readonly properties = computed<P>(() => this.cell().properties() as P);

  public readonly isCellFocused = signal<boolean>(false);
  public readonly isFilteredMatch = signal<boolean>(false);
  public readonly isFilteredActive = computed<boolean>(() => {
    const fragment =
      this._specSheetComparisonFilterService.activeFilterResultReferenceId();
    if (!fragment) {
      return false;
    }

    return fragment === this.cell().referenceId();
  });
  public readonly hasValue = computed(() => !isNil(this.cellValue()));
  public readonly cellValue = computed(() => {
    const cell = this.cell();
    if (!cell) {
      return null;
    }

    return (cell.cellValue as any).value;
  });
  public readonly styling = computed(() => {
    const cell = this.cell();
    if (!cell) {
      return {};
    }

    let backgroundColor: string | undefined =
      cell.stylingConfiguration().backgroundColor;
    if (
      this.isFilteredActive() ||
      this.isFilteredMatch() ||
      this.comparison().state !== 'unchanged'
    ) {
      backgroundColor = undefined;
    }

    return {
      'font-weight': cell.stylingConfiguration().fontWeight,
      'font-size': cell.stylingConfiguration().fontSize,
      backgroundColor: backgroundColor,
    };
  });

  constructor(
    private readonly _specSheetComparisonFilterService: SheetComparisonFilterService,
  ) {
    super();

    effect(() => {
      const filter = this._specSheetComparisonFilterService
        .filter()
        .toLocaleLowerCase();
      if (!filter || filter.trim().length === 0) {
        this.isFilteredMatch.set(false);
        this._specSheetComparisonFilterService.clearMatches();
        return;
      }

      const cellValue = this.cellValue();
      const cellValueString =
        cellValue !== null && cellValue !== undefined
          ? cellValue.toString().toLocaleLowerCase()
          : '';

      if (cellValueString.includes(filter)) {
        this.isFilteredMatch.set(true);
        this._specSheetComparisonFilterService.addMatch({
          referenceId: this.cell()?.referenceId() ?? '',
          tableOrder: 1, //this.table().displayOrder(),
          rowNumber: 1, //this.row().rowNumber(),
          groupOrder: 1, //this.group().order(),
        });
      } else {
        this.isFilteredMatch.set(false);
        this._specSheetComparisonFilterService.removeMatch(
          this.cell()?.referenceId() ?? '',
        );
      }
    });
  }
}
