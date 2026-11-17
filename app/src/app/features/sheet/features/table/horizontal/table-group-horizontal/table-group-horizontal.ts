import {
  CdkDrag,
  CdkDragDrop,
  CdkDragPreview,
  CdkDropList,
  CdkDropListGroup,
  DragDropModule,
  transferArrayItem,
} from '@angular/cdk/drag-drop';
import {
  Component,
  computed,
  HostListener,
  inject,
  input,
  Signal,
} from '@angular/core';
import { Destroyable } from '../../../../../common/destructable/destructable';
import { PUSpecSheetTableGroup } from '../../../../models/pu-spec-sheet-table-group';
import { PUSpecSheetTableRow } from '../../../../models/pu-spec-sheet-table-row';
import { PUSpecSheetTableBase } from '../../../../models/tables/pu-spec-sheet-table-base';
import { ActiveSheetService } from '../../../../services/active-sheet.service';
import { TableRowHorizontalStatic } from '../table-row-horizontal-static/table-row-horizontal-static';
import { TableRowHorizontal } from '../table-row-horizontal/table-row-horizontal';

@Component({
  selector: 'div[app-table-group-horizontal]',
  imports: [
    CdkDragPreview,
    DragDropModule,
    TableRowHorizontal,
    TableRowHorizontalStatic,
  ],
  templateUrl: './table-group-horizontal.html',
  styleUrl: './table-group-horizontal.scss',
  host: {
    '[style.grid-area]': 'gridArea()',
  },
  hostDirectives: [
    { directive: CdkDrag, inputs: ['cdkDragDisabled', 'cdkDragData'] },
    {
      directive: CdkDropList,
      inputs: [
        'cdkDropListDisabled',
        'cdkDropListData',
        'cdkDropListOrientation',
        'cdkDropListLockAxis',
      ],
      outputs: ['cdkDropListDropped'],
    },
    { directive: CdkDropListGroup },
  ],
})
export class TableGroupHorizontal extends Destroyable {
  public readonly table = input.required<PUSpecSheetTableBase>();
  public readonly parentGroup = input.required<PUSpecSheetTableGroup>();
  public readonly group = input.required<PUSpecSheetTableGroup>();
  public readonly allowRowReordering = input.required<boolean>();
  public readonly gridColumn = input<string | undefined>();
  public readonly gridArea = input<string | undefined>();
  public readonly rebuildVersion = input(0);
  public readonly rows = computed(() => {
    this.rebuildVersion();
    return [...this.group().rows()];
  });
  public readonly orderableRows = computed(() =>
    this.rows().filter((r) => r.tableRowTemplate()?.canBeReordered),
  );
  public readonly childGroupLayouts = computed(() => {
    this.rebuildVersion();
    let columnPosition = 2;
    return this.group()
      .childGroups()
      .map((group) => {
        const columnCount = group.columnDefinitions().count;
        const gridArea = `1 / ${columnPosition} / span ${group.rows().length} / span ${columnCount}`;
        columnPosition += columnCount;
        return { group, gridArea };
      });
  });

  @HostListener('cdkDropListDropped', ['$event']) public drop(
    event: CdkDragDrop<PUSpecSheetTableGroup[]>,
  ): void {
    if (event.previousContainer === event.container) {
      this.group().moveGroup(event.previousIndex, event.currentIndex);
    } else {
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex,
      );

      for (let i = 0; i < event.previousContainer.data.length; i++) {
        const group = event.previousContainer.data[i];
        group.order.set(i + 1);
      }

      for (let i = 0; i < event.container.data.length; i++) {
        const group = event.container.data[i];
        group.order.set(i + 1);
      }
    }

    this.table().triggerRebuild();
  }

  private readonly _activeSheetService = inject(ActiveSheetService);

  public dropRows(event: CdkDragDrop<PUSpecSheetTableRow[]>): void {
    if (!this.canEdit() || !this.table().canUserEdit) {
      return;
    }

    const previousRow = this.orderableRows()[event.previousIndex];
    const currentRow = this.orderableRows()[event.currentIndex];
    if (!previousRow || !currentRow) {
      return;
    }

    const indexInGroup = this.group().rows().indexOf(previousRow);
    const targetIndexInGroup = this.group().rows().indexOf(currentRow);

    this.parentGroup().moveRow(indexInGroup, targetIndexInGroup);

    for (const childGroup of this.parentGroup().childGroups()) {
      childGroup.moveRow(indexInGroup, targetIndexInGroup);
    }

    this.table().triggerRebuild();
  }

  public sortPredicate(
    index: number,
    item: CdkDrag<PUSpecSheetTableRow>,
    drop: CdkDropList<PUSpecSheetTableRow[]>,
  ): boolean {
    const itemAtIndex = drop.data[index];
    if (!itemAtIndex) {
      return true;
    }

    return itemAtIndex.tableRowTemplate()?.canBeReordered ?? false;
  }

  public get canEdit(): Signal<boolean> {
    return this._activeSheetService.canEdit;
  }
}
