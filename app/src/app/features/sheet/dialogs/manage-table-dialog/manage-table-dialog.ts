import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { Component, computed, inject, linkedSignal } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { Message } from 'primeng/message';
import { DialogHeader } from '../../../common/dialog-header/dialog-header';
import { InfoBlock } from '../../../common/info-block/info-block';
import { ConfigurableCellGroup } from '../../models/configuration/configurable-cell-group';
import { ConfigurableTableGroup } from '../../models/configuration/configurable-table-group';
import { ConfigurationGroup } from '../../models/configuration/configuration-group';
import { CellPropertiesList } from './components/cells/cell-properties-list/cell-properties-list';
import { ConfigureProperties } from './components/configure-properties/configure-properties';
import { TablePropertiesList } from './components/tables/table-properties-list/table-properties-list';
import { ManageTableDialogData } from './models/manage-table-dialog-data';

@Component({
  selector: 'app-manage-table-dialog',
  imports: [
    DialogHeader,
    ButtonDirective,
    Message,
    DragDropModule,
    CellPropertiesList,
    InfoBlock,
    TablePropertiesList,
    ConfigureProperties,
  ],
  templateUrl: './manage-table-dialog.html',
  styleUrl: './manage-table-dialog.scss',
  host: { class: 'dialog card' },
})
export class ManageTableDialog {
  public readonly selectedGroup = linkedSignal(() => {
    const cellToConfigure = this.data.cellToConfigure;
    if (!cellToConfigure) {
      return this.cells().length > 0 ? this.cells()[0] : undefined;
    }

    const groups = this.cells();
    return groups.find((g) => g.value.cells.includes(cellToConfigure));
  });
  public readonly cells = computed(() =>
    this.data.table.configurableCells().map(
      (cells) =>
        ({
          type: 'cell',
          value: cells,
          uid: cells.uid,
        }) as ConfigurationGroup<ConfigurableCellGroup>,
    ),
  );
  public readonly tableToConfigure = computed<
    ConfigurationGroup<ConfigurableTableGroup> | undefined
  >(() => {
    const data = this.data;
    if (data.table.requiresUserConfiguration()) {
      return {
        type: 'table',
        uid: data.table.referenceId(),
        value: {
          type: 'table',
          table: data.table,
          title: data.table.title(),
          uid: data.table.referenceId(),
        },
      };
    }

    return undefined;
  });
  public readonly data = inject<ManageTableDialogData>(DIALOG_DATA);

  private readonly _dialogRef = inject(DialogRef<ManageTableDialog>);

  public groupSelected(group: ConfigurationGroup<any>): void {
    this.selectedGroup.set(group);
  }

  public close(): void {
    this._dialogRef.close();
  }
}
