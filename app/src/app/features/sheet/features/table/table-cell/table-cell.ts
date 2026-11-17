import { NgClass } from '@angular/common';
import {
  AfterViewInit,
  Component,
  inject,
  input,
  Signal,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { Tag } from 'primeng/tag';
import { FragmentedElementDirective } from '../../../../../core/directives/fragmented-element.directive';
import { Destroyable } from '../../../../common/destructable/destructable';
import { PUSpecSheetTableCell } from '../../../models/cells/pu-spec-sheet-table-cell';
import { PUSpecSheetTableGroup } from '../../../models/pu-spec-sheet-table-group';
import { PUSpecSheetTableRow } from '../../../models/pu-spec-sheet-table-row';
import { PUSpecSheetTableBase } from '../../../models/tables/pu-spec-sheet-table-base';
import { ActiveSheetService } from '../../../services/active-sheet.service';
import { ReferencesIndicator } from '../references-indicator/references-indicator';
import { CellTypeHelpers } from '../table-cell-types/helpers/cell-type-helpers';

@Component({
  selector: 'div[app-table-cell]',
  imports: [Tag, FragmentedElementDirective, NgClass, ReferencesIndicator],
  templateUrl: './table-cell.html',
  styleUrl: './table-cell.scss',
  host: {
    '[style.grid-column]':
      'cell().columnNumber() + " / span " + (cell().colSpan() || 1)',
    '[style.position]':
      'cell().stylingConfiguration().isSticky ? "sticky" : "initial"',
    '[style.z-index]': 'cell().stylingConfiguration().isSticky ? 100 : "auto"',
    '[style.left]':
      'cell().stylingConfiguration().isSticky && table().orientation() === "horizontal" ? "0" : null',
  },
})
export class TableCell extends Destroyable implements AfterViewInit {
  public readonly table = input.required<PUSpecSheetTableBase>();
  public readonly group = input.required<PUSpecSheetTableGroup>();
  public readonly row = input.required<PUSpecSheetTableRow>();
  public readonly cell = input.required<PUSpecSheetTableCell>();
  public readonly shouldShowReferencesIndicator = input.required<boolean>();

  public readonly cellContent = viewChild.required('cellContent', {
    read: ViewContainerRef,
  });

  private readonly _specSheetService = inject(ActiveSheetService);

  ngAfterViewInit(): void {
    this.buildContent();
  }

  public get canEdit(): Signal<boolean> {
    return this._specSheetService.canEdit as any;
  }

  private buildContent(): void {
    const table = this.table();
    const group = this.group();
    const row = this.row();
    const cell = this.cell();
    if (!table || !group || !row || !cell) {
      return;
    }

    const cellContent = this.cellContent();
    if (!cellContent) {
      return;
    }

    cellContent.clear();

    const component = CellTypeHelpers.getCellTypeComponent(
      cell.tableCellType()!.key,
    );
    if (!component) {
      return;
    }

    const componentRef = cellContent.createComponent(component);
    componentRef.setInput('cell', cell);
    componentRef.setInput('table', table);
    componentRef.setInput('group', group);
    componentRef.setInput('row', row);
  }
}
