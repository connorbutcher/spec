import { CdkDragDrop, DragDropModule } from '@angular/cdk/drag-drop';
import {
  AfterViewInit,
  Component,
  computed,
  effect,
  inject,
  input,
  Signal,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { takeUntil } from 'rxjs';
import { Destroyable } from '../../../../../common/destructable/destructable';
import { PUSpecSheetTableGroup } from '../../../../models/pu-spec-sheet-table-group';
import { PUSpecSheetTableRow } from '../../../../models/pu-spec-sheet-table-row';
import { PUSpecSheetTableBase } from '../../../../models/tables/pu-spec-sheet-table-base';
import { ActiveSheetService } from '../../../../services/active-sheet.service';
import { ViewNotes } from '../../notes/view-notes/view-notes';
import { TableHeader } from '../../table-header/table-header';
import { TableGroup } from '../table-group/table-group';

@Component({
  selector: 'app-table',
  imports: [DragDropModule, TableHeader, ViewNotes],
  templateUrl: './table.html',
  styleUrl: './table.scss',
})
export class Table extends Destroyable implements AfterViewInit {
  public readonly table = input.required<PUSpecSheetTableBase>();
  public readonly gridContent = viewChild.required('gridContent', {
    read: ViewContainerRef,
  });

  private readonly _activeSheetService = inject(ActiveSheetService);

  public readonly columnCount = computed(() => {
    const table = this.table();
    if (!table) {
      return 1;
    }

    const defaultGroups = table
      .groups()
      .filter((g) => g.tableGroupTemplate()?.isDefault);
    if (!defaultGroups) {
      return 1;
    }

    // Get the row with the most cells
    let row: PUSpecSheetTableRow | undefined = undefined;
    for (const defaultGroup of defaultGroups) {
      for (const rowTemplate of defaultGroup.rows()) {
        if (rowTemplate.cells().length > (row ? row.cells().length : 0)) {
          row = rowTemplate;
        }
      }
    }

    let columnDefinitions = '';
    for (const cell of row!.cells()) {
      const width = cell.stylingConfiguration().width;
      columnDefinitions += `minmax(${width}em, auto) `;
    }

    return columnDefinitions.trim();
  });

  constructor() {
    super();

    effect(() => {
      const table = this.table();
      const gridContent = this.gridContent();
      if (!table || !gridContent) {
        return;
      }

      this.buildGroups();
    });
  }

  ngAfterViewInit(): void {
    const table = this.table();
    table.triggerRebuild$.pipe(takeUntil(this.destroy)).subscribe(() => {
      this.buildGroups();
    });
  }

  public drop(event: CdkDragDrop<PUSpecSheetTableGroup[]>): void {
    if (event.previousContainer !== event.container) {
      return;
    }

    const table = this.table();
    table.moveGroup(event.previousIndex, event.currentIndex);
  }

  public get canEdit(): Signal<boolean> {
    return this._activeSheetService.canEdit as any;
  }

  private buildGroups(): void {
    const table = this.table();
    if (!table) {
      return;
    }

    const gridContent = this.gridContent();
    if (!gridContent) {
      return;
    }

    gridContent.clear();

    for (const group of table.groups()) {
      const tableGroupComponentRef = gridContent.createComponent(TableGroup);
      tableGroupComponentRef.setInput('table', table);
      tableGroupComponentRef.setInput('group', group);
      tableGroupComponentRef.setInput('parentGroup', group);
      tableGroupComponentRef.setInput(
        'cdkDragDisabled',
        !this.canEdit() || !group.tableGroupTemplate()?.canBeReordered,
      );
      tableGroupComponentRef.setInput('cdkDragData', group);
      tableGroupComponentRef.setInput('cdkDropListDisabled', !this.canEdit());
      tableGroupComponentRef.setInput('cdkDropListData', {
        group,
        childGroups: group.childGroups(),
      });
      tableGroupComponentRef.setInput('cdkDropListOrientation', 'vertical');
      tableGroupComponentRef.setInput('cdkDropListLockAxis', 'y');
      tableGroupComponentRef.setInput(
        'hoverEnabled',
        group.tableGroupTemplate()?.showContextMenu,
      );
    }
  }
}
