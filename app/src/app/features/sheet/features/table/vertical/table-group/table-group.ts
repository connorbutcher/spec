import {
  CdkDrag,
  CdkDragDrop,
  CdkDragPreview,
  CdkDropList,
  CdkDropListGroup,
  DragDropModule,
} from '@angular/cdk/drag-drop';
import {
  AfterViewInit,
  Component,
  HostListener,
  inject,
  input,
  Signal,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { Destroyable } from '../../../../../common/destructable/destructable';
import { RowHoverDirective } from '../../../../directives/row-hover.directive';
import { DropListData } from '../../../../models/drop-list-data';
import { PUSpecSheetTableGroup } from '../../../../models/pu-spec-sheet-table-group';
import { PUSpecSheetTableBase } from '../../../../models/tables/pu-spec-sheet-table-base';
import { ActiveSheetService } from '../../../../services/active-sheet.service';
import { ReferencesIndicator } from '../../references-indicator/references-indicator';
import { TableRow } from '../table-row/table-row';

@Component({
  selector: 'div[app-table-group]',
  imports: [CdkDragPreview, DragDropModule, ReferencesIndicator],
  templateUrl: './table-group.html',
  styleUrl: './table-group.scss',
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
    {
      directive: RowHoverDirective,
      inputs: ['table', 'parentGroup', 'group', 'hoverEnabled'],
    },
  ],
})
export class TableGroup extends Destroyable implements AfterViewInit {
  public readonly table = input.required<PUSpecSheetTableBase>();
  public readonly parentGroup = input.required<PUSpecSheetTableGroup>();
  public readonly group = input.required<PUSpecSheetTableGroup>();
  public readonly gridContent = viewChild.required('gridContent', {
    read: ViewContainerRef,
  });
  @HostListener('cdkDropListDropped', ['$event']) public drop(
    event: CdkDragDrop<DropListData>,
  ): void {
    if (event.previousContainer === event.container) {
      this.group().moveGroup(event.previousIndex, event.currentIndex);
    } else {
      const previousContainerData = event.previousContainer.data;
      const containerData = event.container.data;

      const itemBeingMoved =
        previousContainerData.childGroups[event.previousIndex];
      previousContainerData.group.removeGroup(itemBeingMoved, true);

      containerData.group.addExistingGroup(itemBeingMoved, event.currentIndex);
      itemBeingMoved.parentGroupId.set(containerData.group.id);
    }

    this.table().triggerRebuild();
  }

  private readonly _activeSheetService = inject(ActiveSheetService);

  ngAfterViewInit(): void {
    this.buildContent();
  }

  public get canEdit(): Signal<boolean> {
    return this._activeSheetService.canEdit;
  }

  private buildContent(): void {
    const table = this.table();
    const group = this.group();
    if (!table || !group) {
      return;
    }

    const gridContent = this.gridContent();
    if (!gridContent) {
      return;
    }

    gridContent.clear();

    for (const row of group.rows()) {
      const tableRowComponentRef = gridContent.createComponent(TableRow);
      tableRowComponentRef.setInput('id', row.referenceId());
      tableRowComponentRef.setInput('row', row);
      tableRowComponentRef.setInput('group', group);
      tableRowComponentRef.setInput('table', table);
    }

    for (const childGroup of group.childGroups()) {
      const tableGroupComponentRef = gridContent.createComponent(TableGroup);
      tableGroupComponentRef.setInput('table', table);
      tableGroupComponentRef.setInput('group', childGroup);
      tableGroupComponentRef.setInput('parentGroup', group);
      tableGroupComponentRef.setInput(
        'cdkDragDisabled',
        !this.canEdit() || !childGroup.tableGroupTemplate()?.canBeReordered,
      );
      tableGroupComponentRef.setInput('cdkDragData', childGroup);
      tableGroupComponentRef.setInput('cdkDropListDisabled', !this.canEdit());
      tableGroupComponentRef.setInput('cdkDropListData', {
        group: childGroup,
        childGroups: childGroup.childGroups(),
      });
      tableGroupComponentRef.setInput('cdkDropListOrientation', 'vertical');
      tableGroupComponentRef.setInput('cdkDropListLockAxis', 'y');
      tableGroupComponentRef.setInput(
        'hoverEnabled',
        childGroup.tableGroupTemplate()?.showContextMenu,
      );
    }
  }
}
