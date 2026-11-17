import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { Component, computed, inject, signal } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { Message } from 'primeng/message';
import { TableProperties } from '../../../../core/models/domain/properties/table/table-properties';
import { TableTemplate } from '../../../../core/models/domain/template/table-template';
import { DialogHeader } from '../../../common/dialog-header/dialog-header';
import { ManageTableProperties } from '../../features/table-properties/components/manage-table-properties/manage-table-properties';
import { TablePropertiesClassHelpers } from '../../features/table-properties/helpers/table-properties-class-helper';
import { TablePropertyBase } from '../../features/table-properties/models/table-property-base';

@Component({
  selector: 'app-configure-table-properties-dialog',
  imports: [DialogHeader, ButtonDirective, Message, ManageTableProperties],
  templateUrl: './configure-table-properties-dialog.html',
  styleUrl: './configure-table-properties-dialog.scss',
  host: { class: 'dialog card' },
})
export class ConfigureTablePropertiesDialog {
  public readonly tableProperties = signal<
    TablePropertyBase<TableProperties> | undefined
  >(undefined);
  public readonly isValid = computed(() => this.tableProperties()?.isValid());

  private readonly _data = inject<TableTemplate>(DIALOG_DATA);
  private readonly _dialogRef = inject(
    DialogRef<TableTemplate, ConfigureTablePropertiesDialog>,
  );

  constructor() {
    const tableProperty = TablePropertiesClassHelpers.map(this._data);
    this.tableProperties.set(tableProperty);
  }

  public saveConfiguration(): void {
    const tableTemplates = this.tableProperties()?.toTemplate();
    this._dialogRef.close(tableTemplates);
  }

  public close(): void {
    this._dialogRef.close(undefined);
  }
}
