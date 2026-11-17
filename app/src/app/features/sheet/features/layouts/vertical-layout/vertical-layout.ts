import { CdkDrag, CdkDragDrop, CdkDropList } from '@angular/cdk/drag-drop';
import { Component, inject, Signal } from '@angular/core';
import { InfoBlock } from '../../../../common/info-block/info-block';
import { PUSpecSheet } from '../../../models/pu-spec-sheet';
import { PUSpecSheetTableBase } from '../../../models/tables/pu-spec-sheet-table-base';
import { ActiveSheetService } from '../../../services/active-sheet.service';
import { Table } from '../../table/vertical/table/table';
import { VerticalTableRowHoverContextMenu } from '../../table/vertical/vertical-table-row-context-menu/vertical-table-row-context-menu';

@Component({
  selector: 'app-vertical-layout',
  imports: [
    CdkDropList,
    CdkDrag,
    Table,
    InfoBlock,
    VerticalTableRowHoverContextMenu,
  ],
  templateUrl: './vertical-layout.html',
  styleUrl: './vertical-layout.scss',
})
export class VerticalLayout {
  private readonly _specSheetService = inject(ActiveSheetService);

  public drop(event: CdkDragDrop<PUSpecSheetTableBase[]>): void {
    const specSheet = this.specSheet();
    if (!specSheet) {
      return;
    }

    specSheet.moveTable(event.previousIndex, event.currentIndex);
  }

  public get specSheet(): Signal<PUSpecSheet> {
    return this._specSheetService.specSheet;
  }

  public get canEdit(): Signal<boolean> {
    return this._specSheetService.canEdit;
  }
}
