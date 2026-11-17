import { Component, computed } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { Tooltip } from 'primeng/tooltip';
import { CellTypeBase } from '../cell-type-base';
import { TextCellType } from '../text-cell-type/text-cell-type';

@Component({
  selector: 'app-parts-part-number-cell-type',
  imports: [TextCellType, ButtonDirective, Tooltip],
  templateUrl: './parts-part-number-cell-type.html',
  styleUrl: './parts-part-number-cell-type.scss',
  host: { class: 'cell-host' },
})
export class PartsPartNumberCellType extends CellTypeBase<string> {
  public readonly addOptions = computed(() => {
    const table = this.table();
    const group = this.group();
    if (!table || !group) {
      return [];
    }

    const tableType = table.tableType();
    return tableType.tableGroupTemplates.filter(
      (tg) => tg.parentTableGroupTemplateId === group.tableGroupTemplate()?.id,
    );
  });

  public deletePart(): void {
    const table = this.table();
    const group = this.group();
    const parentGroup = table
      .groups()
      .find((g) => g.id === group.parentGroupId());

    if (!table || !group || !parentGroup) {
      return;
    }

    parentGroup.removeGroup(group, false);
  }
}
