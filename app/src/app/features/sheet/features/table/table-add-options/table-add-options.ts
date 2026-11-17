import { Component, inject, input, Signal } from '@angular/core';
import { TableGroupTemplate } from '../../../../../core/models/domain/template/table-group-template';
import { PUSpecSheetTableGroup } from '../../../models/pu-spec-sheet-table-group';
import { PUSpecSheetTableBase } from '../../../models/tables/pu-spec-sheet-table-base';
import { ActiveSheetService } from '../../../services/active-sheet.service';
import { TableGroupBuilderService } from '../../../services/table-group-builder.service';

@Component({
  selector: 'app-table-add-options',
  templateUrl: './table-add-options.html',
  styleUrl: './table-add-options.scss',
  providers: [TableGroupBuilderService],
})
export class TableAddOptions {
  public readonly options = input.required<TableGroupTemplate[]>();
  public readonly table = input.required<PUSpecSheetTableBase>();
  public readonly parentGroup = input<PUSpecSheetTableGroup>();

  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _tableGroupBuilderService = inject(TableGroupBuilderService);

  public async addOptionAsync(
    tableGroupTemplate: TableGroupTemplate,
  ): Promise<void> {
    await this._tableGroupBuilderService.addOptionAsync(
      tableGroupTemplate,
      this.table(),
      this.parentGroup(),
    );
  }

  public get canEdit(): Signal<boolean> {
    return this._activeSheetService.canEdit;
  }
}
