import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { Component, computed, inject, signal } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { Message } from 'primeng/message';
import { CellProperties } from '../../../../core/models/domain/properties/cell/cell-properties';
import { TableCellTemplate } from '../../../../core/models/domain/template/table-cell-template';
import { DialogHeader } from '../../../common/dialog-header/dialog-header';
import { ManageCellProperties } from '../../features/cell-properties/components/manage-cell-properties/manage-cell-properties';
import { CellPropertiesClassHelpers } from '../../features/cell-properties/helpers/cell-properties-class-helper';
import { CellPropertyBase } from '../../features/cell-properties/models/cell-property-base';

@Component({
  selector: 'app-configure-cell-properties-dialog',
  imports: [DialogHeader, ButtonDirective, Message, ManageCellProperties],
  templateUrl: './configure-cell-properties-dialog.html',
  styleUrl: './configure-cell-properties-dialog.scss',
  host: { class: 'dialog card' },
})
export class ConfigureCellPropertiesDialog {
  public readonly cellProperties = signal<CellPropertyBase<CellProperties>[]>(
    [],
  );
  public readonly isValid = computed(() =>
    this.cellProperties().every((cp) => cp.isValid()),
  );

  private readonly _data = inject<TableCellTemplate[]>(DIALOG_DATA);
  private readonly _dialogRef = inject(
    DialogRef<TableCellTemplate[], ConfigureCellPropertiesDialog>,
  );

  constructor() {
    for (const cellTemplate of this._data) {
      const cellProperty =
        CellPropertiesClassHelpers.mapToClassFromConfigure(cellTemplate);
      this.cellProperties.update((v) => [...v, cellProperty]);
    }
  }

  public saveConfiguration(): void {
    const cellTemplates = this.cellProperties().map((cp) =>
      cp.toCellTemplate(),
    );

    this._dialogRef.close(cellTemplates);
  }

  public close(): void {
    this._dialogRef.close(undefined);
  }
}
