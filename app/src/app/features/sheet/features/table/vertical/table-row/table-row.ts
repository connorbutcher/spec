import {
  AfterViewInit,
  Component,
  effect,
  inject,
  input,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { FragmentedElementDirective } from '../../../../../../core/directives/fragmented-element.directive';
import { ActivationService } from '../../../../../../core/services/activation.service';
import { PUCardAnalyticsService } from '../../../../../analytics/services/pu-card-analytics.service';
import { Destroyable } from '../../../../../common/destructable/destructable';
import { PUSpecSheetTableGroup } from '../../../../models/pu-spec-sheet-table-group';
import { PUSpecSheetTableRow } from '../../../../models/pu-spec-sheet-table-row';
import { PUSpecSheetTableBase } from '../../../../models/tables/pu-spec-sheet-table-base';
import { SelectionService } from '../../../../services/selection.service';
import { ReferencesIndicator } from '../../references-indicator/references-indicator';
import { TableCell } from '../../table-cell/table-cell';

@Component({
  selector: 'div[app-table-row]',
  imports: [FragmentedElementDirective, ReferencesIndicator],
  templateUrl: './table-row.html',
  styleUrl: './table-row.scss',
  host: {
    '[style.grid-row]': 'row().rowNumber()',
    '[class.invalid-row]': '!row().isValid()',
    '[class.has-references]': '(row().rowReferences()?.length ?? 0) > 0',
    '[class]': 'row().advisoryStateClass()',
    '(click)': 'selectRow()',
  },
  hostDirectives: [{ directive: FragmentedElementDirective, inputs: ['id'] }],
})
export class TableRow extends Destroyable implements AfterViewInit {
  public readonly table = input.required<PUSpecSheetTableBase>();
  public readonly group = input.required<PUSpecSheetTableGroup>();
  public readonly row = input.required<PUSpecSheetTableRow>();
  public readonly id = input.required<string>();

  public readonly rowContent = viewChild.required('rowContent', {
    read: ViewContainerRef,
  });

  private readonly _activationService = inject(ActivationService);
  private readonly _puCardAnalyticsService = inject(PUCardAnalyticsService);
  private readonly _selectionService = inject(SelectionService);

  constructor() {
    super();

    effect(() => {
      const refToActivate = this._activationService.refToActivate();
      if (!refToActivate || refToActivate !== this.id()) {
        return;
      }

      this._activationService.refToActivate.set(undefined);
    });
  }

  ngAfterViewInit(): void {
    this.buildContent();
  }

  public selectRow(): void {
    // Only allow one 1 row to be selected at a time.
    if (!this._puCardAnalyticsService.isEnabled()) {
      return;
    }

    const anySelected = this.row()
      .cells()
      .some((cell) => this._selectionService.isSelected(cell));
    if (!anySelected) {
      // If none of the cells in this row are selected, reset the selection and prepare to select this row's cells.
      this._selectionService.reset();
    }

    for (const cell of this.row().cells()) {
      if (this._selectionService.isSelected(cell)) {
        this._selectionService.unselect(cell);
        continue;
      }

      this._selectionService.select(cell);
    }
  }

  private buildContent(): void {
    const table = this.table();
    const group = this.group();
    const row = this.row();
    if (!table || !group || !row) {
      return;
    }

    const rowContent = this.rowContent();
    if (!rowContent) {
      return;
    }

    rowContent.clear();

    for (const cell of row.cells()) {
      const tableCellComponentRef = rowContent.createComponent(TableCell);
      tableCellComponentRef.setInput('table', table);
      tableCellComponentRef.setInput('group', group);
      tableCellComponentRef.setInput('row', row);
      tableCellComponentRef.setInput('cell', cell);
      tableCellComponentRef.setInput('shouldShowReferencesIndicator', false);
    }
  }
}
