import {
  Component,
  computed,
  effect,
  inject,
  Injector,
  input,
  model,
  signal,
} from '@angular/core';
import { isNil } from 'lodash';
import { CellProperties } from '../../../../../core/models/domain/properties/cell/cell-properties';
import { ActivationService } from '../../../../../core/services/activation.service';
import { FragmentHandler } from '../../../../../core/services/fragment-handler.service';
import { Destroyable } from '../../../../common/destructable/destructable';
import { PUSpecSheetTableCell } from '../../../models/cells/pu-spec-sheet-table-cell';
import { PUSpecSheetTableGroup } from '../../../models/pu-spec-sheet-table-group';
import { PUSpecSheetTableRow } from '../../../models/pu-spec-sheet-table-row';
import { PUSpecSheetTableBase } from '../../../models/tables/pu-spec-sheet-table-base';
import { ActiveSheetService } from '../../../services/active-sheet.service';
import { SelectionService } from '../../../services/selection.service';
import { SheetFilterService } from '../../../services/sheet-filter.service';
import { CellPropertyBase } from '../../cell-properties/models/cell-property-base';

@Component({
  selector: 'app-cell-type-base',
  template: '',
  standalone: true,
  host: {
    '[style]': 'styling()',
    '[class.filtered-match]': 'isFilteredMatch()',
    '[class.filtered-active]': 'isFilteredActive()',
    '[class.selected-row]': 'isSelected()',
  },
})
export class CellTypeBase<
  T,
  C extends PUSpecSheetTableCell = PUSpecSheetTableCell,
  P extends CellPropertyBase<CellProperties> | undefined = undefined,
> extends Destroyable {
  public readonly cellValue = model<T>();

  public readonly cell = input.required<C>();
  public readonly properties = computed<P>(() => this.cell().properties() as P);
  public readonly group = input.required<PUSpecSheetTableGroup>();
  public readonly row = input.required<PUSpecSheetTableRow>();
  public readonly table = input.required<PUSpecSheetTableBase>();
  public readonly isSelected = computed(() => {
    const cell = this.cell();
    const selected = this._selectionService.selectedCells();
    return selected.some((s) => s.id === cell.id);
  });

  public readonly isCellFocused = signal<boolean>(false);
  public readonly isFilteredMatch = signal<boolean>(false);
  public readonly isFilteredActive = computed<boolean>(() => {
    const fragment = this._sheetFilterService.activeFilterResultReferenceId();
    if (!fragment) {
      return false;
    }

    return fragment === this.cell().referenceId();
  });
  public readonly hasValue = computed(() => !isNil(this.cellValue()));
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
      this.isSelected()
    ) {
      backgroundColor = undefined;
    }

    return {
      'font-weight': cell.stylingConfiguration().fontWeight,
      'font-size': cell.stylingConfiguration().fontSize,
      backgroundColor: backgroundColor,
    };
  });
  public readonly canEdit = computed(() => this._sheetService.canEdit());

  protected readonly _sheetFilterService = inject(SheetFilterService);
  protected readonly _sheetService = inject(ActiveSheetService);
  protected readonly _selectionService = inject(SelectionService);
  protected readonly _activationService = inject(ActivationService);
  protected readonly _fragmentHandler = inject(FragmentHandler);
  protected readonly _injector = inject(Injector);

  constructor() {
    super();

    effect(() => {
      const filter = this._sheetFilterService.filter().toLocaleLowerCase();
      if (!filter || filter.trim().length === 0) {
        this.isFilteredMatch.set(false);
        this._sheetFilterService.clearMatches();
        return;
      }

      const cellValue = this.cellValue();
      const isMatch = this.isMatch(cellValue, filter);

      if (isMatch) {
        this.isFilteredMatch.set(true);
        this._sheetFilterService.addMatch({
          referenceId: this.cell().referenceId(),
          tableOrder: this.table().displayOrder(),
          groupOrder: this.group().order(),
          rowNumber: this.row().rowNumber(),
        });
      } else {
        this.isFilteredMatch.set(false);
        this._sheetFilterService.removeMatch(this.cell().referenceId());
      }
    });

    effect(() => {
      const refToActivate = this._activationService.refToActivate();
      if (!refToActivate || refToActivate !== this.cell().referenceId()) {
        return;
      }

      this._activationService.refToActivate.set(undefined);
      this.focusCell(new Event('fragment-activated'));
    });
  }

  public focusCell(event: any): void {
    if (!this.canEdit()) {
      return;
    }

    if (!event) {
      this.isCellFocused.set(false);
      return;
    }

    this.isCellFocused.set(true);
  }

  public isMatch(cellValue: any, filter: string): boolean {
    if (cellValue === null || cellValue === undefined) {
      return false;
    }

    const cellValueString = cellValue.toString().toLocaleLowerCase();
    return cellValueString.includes(filter);
  }
}
