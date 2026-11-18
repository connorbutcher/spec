import { Dialog } from '@angular/cdk/dialog';
import { CdkDragHandle } from '@angular/cdk/drag-drop';
import {
  Component,
  computed,
  inject,
  Injector,
  input,
  Signal,
} from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { OverlayBadge } from 'primeng/overlaybadge';
import { Popover } from 'primeng/popover';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';
import { firstValueFrom } from 'rxjs';
import { FragmentedElementDirective } from '../../../../../core/directives/fragmented-element.directive';
import { ConfirmationDialog } from '../../../../common/confirmation-dialog/confirmation-dialog';
import { ConfirmationDialogData } from '../../../../common/confirmation-dialog/models/confirmation-dialog-data.interface';
import { ConfigureTableAccessDialog } from '../../../dialogs/configure-table-access-dialog/configure-table-access-dialog';
import { ConfigureTableAccessDialogData } from '../../../dialogs/configure-table-access-dialog/models/configure-table-access-dialog-data';
import { ManageTableDialog } from '../../../dialogs/manage-table-dialog/manage-table-dialog';
import { ManageTableDialogData } from '../../../dialogs/manage-table-dialog/models/manage-table-dialog-data';
import { SystemReferencesConfirmationDialogData } from '../../../dialogs/system-references-confirmation-dialog/models/system-references-confirmation-dialog-data.interface';
import { SystemReferencesConfirmationDialog } from '../../../dialogs/system-references-confirmation-dialog/system-references-confirmation-dialog';
import { PUSpecSheetTableBase } from '../../../models/tables/pu-spec-sheet-table-base';
import { ActiveSheetService } from '../../../services/active-sheet.service';
import { SheetCheckoutService } from '../../../services/sheet-checkout.service';
import { ItemLockTag } from '../item-lock-tag/item-lock-tag';
import { ReferencesIndicator } from '../references-indicator/references-indicator';
import { TableAddOptions } from '../table-add-options/table-add-options';
import { TableTitleInput } from '../table-title-input/table-title-input';

@Component({
  selector: 'app-table-header',
  imports: [
    TableTitleInput,
    Tooltip,
    Popover,
    ButtonDirective,
    OverlayBadge,
    Tag,
    FragmentedElementDirective,
    CdkDragHandle,
    TableAddOptions,
    ReferencesIndicator,
    ItemLockTag,
  ],
  templateUrl: './table-header.html',
  styleUrl: './table-header.scss',
})
export class TableHeader {
  public readonly table = input.required<PUSpecSheetTableBase>();
  public readonly defaultGroup = computed(() => {
    const table = this.table();
    if (!table) {
      return undefined;
    }

    return table
      .groups()
      .find((g) => g.tableGroupTemplate()?.isDefault && !g.parentGroupId());
  });
  public readonly addOptions = computed(() => {
    const table = this.table();
    const defaultTemplateGroups = table
      .tableType()
      .tableGroupTemplates.find((tg) => !tg.parentTableGroupTemplateId);

    return table
      .tableType()
      .tableGroupTemplates.filter(
        (tg) =>
          tg.canUserAddToTable &&
          tg.parentTableGroupTemplateId === defaultTemplateGroups?.id,
      );
  });

  /** Whether the table itself (title, configuration, access, removal) can be changed: nobody else has it checked out. */
  public readonly canEditTable = computed(() =>
    this._activeSheetService.canEditItem(this.table().table.revision),
  );
  /** Whether the current user has the table checked out, and can undo their changes to it. */
  public readonly canUndoTable = computed(() => {
    const lock = this._activeSheetService.lockOf(this.table().table.revision);
    return this.canEdit() && !!lock?.isMine && !lock.isAwaitingApproval;
  });

  public readonly canConfigureTable = computed(() => {
    if (!this.canEditTable()) {
      return false;
    }

    const table = this.table();
    if (!table) {
      return false;
    }

    return (
      table.requiresUserConfiguration() || table.configurableCells().length > 0
    );
  });

  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _sheetCheckoutService = inject(SheetCheckoutService);
  private readonly _dialog = inject(Dialog);
  private readonly _injector = inject(Injector);

  public async configureTableAccessAsync(): Promise<void> {
    const dialogRef = this._dialog.open<
      boolean,
      ConfigureTableAccessDialogData,
      ConfigureTableAccessDialog
    >(ConfigureTableAccessDialog, {
      data: {
        table: this.table(),
      },
    });

    const result = await firstValueFrom(dialogRef.closed);
    if (!result) {
      return;
    }

    this._activeSheetService.triggerReload();
  }

  public openManagementDialog(): void {
    const table = this.table();
    if (!table || !this.canConfigureTable()) {
      return;
    }

    this._dialog.open<unknown, ManageTableDialogData, ManageTableDialog>(
      ManageTableDialog,
      {
        data: { table },
        injector: this._injector,
        minWidth: '40vw',
      },
    );
  }

  public async undoTableAsync(): Promise<void> {
    if (!this.canUndoTable()) {
      return;
    }

    await this._sheetCheckoutService.undoTableAsync(this.table().id);
  }

  public async deleteTableAsync(): Promise<void> {
    const table = this.table();
    const specSheet = this._activeSheetService.specSheet();
    if (!specSheet || !table) {
      return;
    }

    let dialogRef;
    if (table.references() && table.references().length > 0) {
      dialogRef = this._dialog.open<
        boolean,
        SystemReferencesConfirmationDialogData,
        SystemReferencesConfirmationDialog
      >(SystemReferencesConfirmationDialog, {
        data: {
          type: 'Table',
          references: table.references(),
        },
      });
    } else {
      dialogRef = this._dialog.open<
        boolean,
        ConfirmationDialogData,
        ConfirmationDialog
      >(ConfirmationDialog, {
        data: {
          title: `Delete Table - ${table.title().toLocaleUpperCase()}`,
          message: `Are you sure you want to delete this table - ${table.title().toLocaleUpperCase()}? This action cannot be undone.`,
          confirmText: 'Delete',
          confirmSeverity: 'danger',
        },
      });
    }

    const result = await firstValueFrom(dialogRef.closed);
    if (!result) {
      return;
    }

    specSheet.deleteTable(table);
  }

  public get canEdit(): Signal<boolean> {
    return this._activeSheetService.canEdit;
  }
}
