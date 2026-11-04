import { Component, computed, inject } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { SheetSection } from '../models/sheet-section.model';
import { SheetTable } from '../models/sheet-table.model';
import { SheetActionTargetUtil } from '../sheet-action-target.util';
import { SheetLabelsUtil } from '../sheet-labels.util';
import { SheetLockUtil } from '../sheet-lock.util';
import { SheetStore } from '../sheet.store';

/**
 * The sheet's one add button. Its menu lists everything that can be added from where the selection is,
 * under a heading for each place: the section around the selection that takes sections, the selected
 * table (its sections and, for a horizontal table, its column blocks), and a new table from one of the
 * sheet type's templates. Sections at their template maximum are shown but can't be picked.
 */
@Component({
  selector: 'app-sheet-context-add',
  imports: [ButtonModule, MenuModule],
  templateUrl: './sheet-context-add.html',
  styleUrl: './sheet-context-add.scss',
})

export class SheetContextAdd {
  public readonly items = computed<MenuItem[]>(() =>
    [this.sectionGroup(), this.tableGroup(), this.newTableGroup()].filter(
      (group) => (group.items?.length ?? 0) > 0,
    ),
  );

  public readonly hasChoices = computed(() => this.items().length > 0);

  public readonly isBusy = computed(() => this.store.isBusy());

  /** The selected table, unless someone else has a change to it in progress. */
  private readonly table = computed<SheetTable | null>(() => {
    const table = this.store.index().tables.get(this.store.selection()?.tableId ?? -1);
    return table !== undefined && SheetLockUtil.otherUsersLock(table.lock) === null ? table : null;
  });

  private readonly section = computed<SheetSection | null>(() =>
    SheetActionTargetUtil.addableSection(this.store.index(), this.store.selection()),
  );

  private readonly sectionGroup = computed<MenuItem>(() => {
    const section = this.section();
    const tableId = this.store.selection()?.tableId;
    if (section === null || tableId === undefined) {
      return {};
    }
    return {
      label: `In ${SheetLabelsUtil.sectionLabel(section)}`,
      items: section.addableSections.map((addable) => ({
        label: addable.name,
        icon: 'pi pi-table',
        disabled: !addable.canAdd,
        command: () => void this.store.addSection(tableId, addable.templateSectionId, section.id),
      })),
    };
  });

  private readonly tableGroup = computed<MenuItem>(() => {
    const table = this.table();
    if (table === null) {
      return {};
    }
    return {
      label: `In ${SheetLabelsUtil.tableLabel(table)}`,
      items: [
        ...table.addableSections.map((addable) => ({
          label: addable.name,
          icon: 'pi pi-table',
          disabled: !addable.canAdd,
          command: () => void this.store.addSection(table.id, addable.templateSectionId, null),
        })),
        ...table.addableColumnBlocks.map((addable) => ({
          label: addable.name,
          icon: 'pi pi-arrows-h',
          disabled: !addable.canAdd,
          command: () => void this.store.addColumnBlock(table.id, addable.templateColumnBlockId),
        })),
      ],
    };
  });

  private readonly newTableGroup = computed<MenuItem>(() => ({
    label: 'New table',
    items: (this.store.sheet()?.availableTemplates ?? []).map((template) => ({
      label: template.name,
      icon: 'pi pi-plus',
      command: () => void this.store.addTable(template.id),
    })),
  }));

  private readonly store = inject(SheetStore);
}
