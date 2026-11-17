import { Dialog } from '@angular/cdk/dialog';
import { Component, computed, inject } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { Popover } from 'primeng/popover';
import { Tooltip } from 'primeng/tooltip';
import { firstValueFrom } from 'rxjs';
import { WidgetTypeKey } from '../../../../../../core/models/domain/enums/widget-type-key';
import { SystemReferencesConfirmationDialogData } from '../../../../dialogs/system-references-confirmation-dialog/models/system-references-confirmation-dialog-data.interface';
import { SystemReferencesConfirmationDialog } from '../../../../dialogs/system-references-confirmation-dialog/system-references-confirmation-dialog';
import { PFKNomWidget } from '../../../../models/widgets/pfk-nominal-widget';
import { TableAddOptions } from '../../table-add-options/table-add-options';
import { PFKNominalWidget } from '../../widgets/pfk-nominal-widget/pfk-nominal-widget';
import { CellTypeBase } from '../cell-type-base';
import { TextCellType } from '../text-cell-type/text-cell-type';

@Component({
  selector: 'app-pfk-part-number-cell-type',
  imports: [
    TextCellType,
    TableAddOptions,
    ButtonDirective,
    Tooltip,
    Popover,
    PFKNominalWidget,
  ],
  templateUrl: './pfk-part-number-cell-type.html',
  styleUrl: './pfk-part-number-cell-type.scss',
  host: { class: 'cell-host' },
})
export class PFKPartNumberCellType extends CellTypeBase<string> {
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
  public readonly nominalWidget = computed(() => {
    const group = this.group();
    if (!group) {
      return undefined;
    }

    return group
      .widgets()
      .find((w) => w.widgetType()?.key === WidgetTypeKey.PFKNominal) as
      | PFKNomWidget
      | undefined;
  });

  private readonly _dialog = inject(Dialog);

  public async deletePfk(): Promise<void> {
    if (!this.canEdit()) {
      return;
    }

    const group = this.group();
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

    parentGroup.removeGroup(group, false);
  }
}
