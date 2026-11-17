import { CdkDrag } from '@angular/cdk/drag-drop';
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
import { Destroyable } from '../../../../../common/destructable/destructable';
import { RowHoverDirective } from '../../../../directives/row-hover.directive';
import { PUSpecSheetTableGroup } from '../../../../models/pu-spec-sheet-table-group';
import { PUSpecSheetTableRow } from '../../../../models/pu-spec-sheet-table-row';
import { PUSpecSheetTableBase } from '../../../../models/tables/pu-spec-sheet-table-base';
import { TableCell } from '../../table-cell/table-cell';

@Component({
  selector: 'div[app-table-row-horizontal]',
  templateUrl: './table-row-horizontal.html',
  styleUrl: './table-row-horizontal.scss',
  host: {
    '[style.grid-row]': 'row().rowNumber()',
    '[class.invalid-row]': '!row().isValid()',
    '[class]': 'row().advisoryStateClass()',
  },
  hostDirectives: [
    { directive: CdkDrag, inputs: ['cdkDragDisabled', 'cdkDragData'] },
    {
      directive: RowHoverDirective,
      inputs: ['table', 'parentGroup', 'group', 'row', 'hoverEnabled'],
    },
    { directive: FragmentedElementDirective, inputs: ['id'] },
  ],
})
export class TableRowHorizontal extends Destroyable implements AfterViewInit {
  public readonly table = input.required<PUSpecSheetTableBase>();
  public readonly group = input.required<PUSpecSheetTableGroup>();
  public readonly parentGroup = input.required<PUSpecSheetTableGroup>();
  public readonly row = input.required<PUSpecSheetTableRow>();
  public readonly id = input.required<string>();

  public readonly rowContent = viewChild.required('rowContent', {
    read: ViewContainerRef,
  });

  private readonly _activationService = inject(ActivationService);

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
      tableCellComponentRef.setInput('shouldShowReferencesIndicator', true);
    }
  }
}
