import { Dialog } from '@angular/cdk/dialog';
import { Component, computed, inject } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { Popover } from 'primeng/popover';
import { Tooltip } from 'primeng/tooltip';
import { firstValueFrom } from 'rxjs';
import { SystemReferencesConfirmationDialogData } from '../../../../dialogs/system-references-confirmation-dialog/models/system-references-confirmation-dialog-data.interface';
import { SystemReferencesConfirmationDialog } from '../../../../dialogs/system-references-confirmation-dialog/system-references-confirmation-dialog';
import { TableAddOptions } from '../../table-add-options/table-add-options';
import { CellTypeBase } from '../cell-type-base';
import { TextCellType } from '../text-cell-type/text-cell-type';

@Component({
  selector: 'app-group-cell-type',
  imports: [TextCellType, TableAddOptions, ButtonDirective, Tooltip, Popover],
  templateUrl: './group-cell-type.html',
  styleUrl: './group-cell-type.scss',
  host: { class: 'cell-host' },
})
export class GroupCellType extends CellTypeBase<string> {
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

  private readonly _dialog = inject(Dialog);

  public async deleteGroup(): Promise<void> {
    if (!this.canEdit()) {
      return;
    }

    const table = this.table();
    const parentGroup = table
      .groups()
      .find((g) => g.id === this.group().parentGroupId());
    if (!parentGroup) {
      return;
    }

    if (this.group().references() && this.group().references().length > 0) {
      const dialogRef = this._dialog.open<
        boolean,
        SystemReferencesConfirmationDialogData,
        SystemReferencesConfirmationDialog
      >(SystemReferencesConfirmationDialog, {
        data: {
          type: 'Group',
          references: this.group().references(),
        },
      });

      const result = await firstValueFrom(dialogRef.closed);
      if (!result) {
        return;
      }
    }

    parentGroup.removeGroup(this.group(), false);
  }
}
