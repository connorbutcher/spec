import { CdkDrag, CdkDragDrop, CdkDropList } from '@angular/cdk/drag-drop';
import { Component, inject, Signal } from '@angular/core';
import { InfoBlock } from '../../../../common/info-block/info-block';
import { PUSpecSheet } from '../../../models/pu-spec-sheet';
import { PUSpecSheetTableBase } from '../../../models/tables/pu-spec-sheet-table-base';
import { ActiveSheetService } from '../../../services/active-sheet.service';
import { HorizontalTableRowHoverContextMenu } from '../../table/horizontal/horizontal-table-row-context-menu/horizontal-table-row-context-menu';
import { TableHorizontal } from '../../table/horizontal/table-horizontal/table-horizontal';

@Component({
  selector: 'app-horizontal-layout',
  imports: [
    CdkDrag,
    CdkDropList,
    InfoBlock,
    TableHorizontal,
    HorizontalTableRowHoverContextMenu,
  ],
  templateUrl: './horizontal-layout.html',
  styleUrl: './horizontal-layout.scss',
})
export class HorizontalLayout {
  private readonly _specSheetService = inject(ActiveSheetService);

  public drop(event: CdkDragDrop<PUSpecSheetTableBase[]>): void {
    const specSheet = this._specSheetService.specSheet();
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
