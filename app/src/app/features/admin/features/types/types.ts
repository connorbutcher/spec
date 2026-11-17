import { Dialog } from '@angular/cdk/dialog';
import {
  Component,
  inject,
  Injector,
  model,
  Signal,
  viewChild,
} from '@angular/core';
import { TreeDragDropService } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Table, TableModule, TableRowSelectEvent } from 'primeng/table';
import { firstValueFrom } from 'rxjs';
import { SpecSheetType } from '../../../../core/models/domain/types/spec-sheet-type';
import { TypesService } from '../../../../core/services/types.service';
import { CreateSpecSheetTypeDialog } from './dialogs/create-spec-sheet-type-dialog/create-spec-sheet-type-dialog';
import { UpdateSpecSheetTypeDialogData } from './dialogs/update-spec-sheet-type-dialog/models/update-spec-sheet-type-dialog-data';
import { UpdateSpecSheetTypeDialog } from './dialogs/update-spec-sheet-type-dialog/update-spec-sheet-type-dialog';
import { TypesDataService } from './types-data.service';

@Component({
  selector: 'app-types',
  imports: [InputText, TableModule, ButtonDirective],
  templateUrl: './types.html',
  styleUrl: './types.scss',
  providers: [TreeDragDropService, TypesDataService],
})
export class Types {
  public readonly table = viewChild.required<Table>(Table);
  public readonly selectedType = model<SpecSheetType>();

  private readonly _typesService = inject(TypesService);
  private readonly _dialog = inject(Dialog);
  private readonly _injector = inject(Injector);

  public filterTable(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.table()?.filterGlobal(filterValue, 'contains');
  }

  public async onRowSelect(event: TableRowSelectEvent) {
    if (!event.data) {
      return;
    }

    const dialogRef = this._dialog.open<
      boolean,
      UpdateSpecSheetTypeDialogData,
      UpdateSpecSheetTypeDialog
    >(UpdateSpecSheetTypeDialog, {
      data: {
        type: event.data,
      },
      injector: this._injector,
    });

    await firstValueFrom(dialogRef.closed);
    this.selectedType.set(undefined);
  }

  public addType() {
    this._dialog.open<void, void, CreateSpecSheetTypeDialog>(
      CreateSpecSheetTypeDialog,
      {
        injector: this._injector,
        minWidth: '55vw',
      },
    );
  }

  public get specSheetTypes(): Signal<SpecSheetType[]> {
    return this._typesService.specSheetTypes;
  }
}
