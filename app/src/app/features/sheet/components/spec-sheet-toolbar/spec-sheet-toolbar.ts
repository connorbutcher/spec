import { Dialog } from '@angular/cdk/dialog';
import { Component, computed, inject, Injector, Signal } from '@angular/core';
import { DateTime } from 'luxon';
import { MenuItem, MessageService } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { SplitButton } from 'primeng/splitbutton';
import { Tooltip } from 'primeng/tooltip';
import { firstValueFrom } from 'rxjs';
import { CellValueHelpers } from '../../../../core/helpers/cell-value-helpers';
import { nextTempId } from '../../../../core/helpers/temp-id-counter';
import { TableTemplate } from '../../../../core/models/domain/template/table-template';
import { PublishSheetRequest } from '../../../../core/models/requests/sheets/publish-sheet-request';
import { UserService } from '../../../../core/services/user.service';
import { PUCardAnalyticsService } from '../../../analytics/services/pu-card-analytics.service';
import { SpinnerDialogData } from '../../../common/spinner-dialog/models/spinner-dialog-data';
import { SpinnerDialog } from '../../../common/spinner-dialog/spinner-dialog';
import { SheetsService } from '../../../sheets/services/sheets.service';
import { ConfigureTablePropertiesDialog } from '../../dialogs/configure-table-properties-dialog/configure-table-properties-dialog';
import { PublishSpecSheetDialog } from '../../dialogs/publish-spec-sheet-dialog/publish-spec-sheet-dialog';
import { AdvisoryOverview } from '../../features/advisories/advisory-overview/advisory-overview';
import { ReferencesButton } from '../../features/references/references-button/references-button';
import { PUSpecSheet } from '../../models/pu-spec-sheet';
import { ActiveSheetService } from '../../services/active-sheet.service';
import { SheetDataService } from '../../services/sheet-data.service';
import { ConfirmationDialog } from '../../../common/confirmation-dialog/confirmation-dialog';
import { ConfirmationDialogData } from '../../../common/confirmation-dialog/models/confirmation-dialog-data.interface';
import { SheetCheckoutService } from '../../services/sheet-checkout.service';
import { SheetApprovalStatus } from '../sheet-approval-alert/sheet-approval-status';
import { SheetCheckoutStatus } from '../sheet-checkout-status/sheet-checkout-status';
import { SpecSheetSearch } from '../spec-sheet-search/spec-sheet-search';
import { SheetItemHelpers } from '../../../../core/helpers/sheet-item-helpers';
import { SheetTable } from '../../../../core/models/domain/sheets/sheet-table';
import { SheetTableCell } from '../../../../core/models/domain/sheets/sheet-table-cell';
import { SheetTableGroup } from '../../../../core/models/domain/sheets/sheet-table-group';
import { SheetTableRow } from '../../../../core/models/domain/sheets/sheet-table-row';
import { SheetWidget } from '../../../../core/models/domain/sheets/sheet-widget';

@Component({
  selector: 'app-spec-sheet-toolbar',
  imports: [
    ButtonDirective,
    SpecSheetSearch,
    SplitButton,
    ReferencesButton,
    AdvisoryOverview,
    SheetApprovalStatus,
    SheetCheckoutStatus,
    Tooltip,
  ],
  templateUrl: './spec-sheet-toolbar.html',
  styleUrl: './spec-sheet-toolbar.scss',
})
export class SpecSheetToolbar {
  public readonly tableTypes = computed(() => {
    const tableTemplates = this._sheetsService.selectedType()?.tableTemplates;
    if (!tableTemplates) {
      return [];
    }

    return tableTemplates.filter((tt) => !tt.isLegacy);
  });
  public readonly tableOptions = computed<MenuItem[]>(() => {
    const tableTypes = this.tableTypes();
    return tableTypes.map(
      (tableType) =>
        ({
          label: `Add ${tableType.type} Table`,
          command: () => this.addTable(tableType),
        }) as MenuItem,
    );
  });
  /** Whether the current user has saved changes checked out that they can undo. */
  public readonly canUndoMine = computed(
    () => this._activeSheetService.checkoutSummary().mine > 0,
  );

  private readonly _sheetsService = inject(SheetsService);
  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _userService = inject(UserService);
  private readonly _puCardAnalyticsService = inject(PUCardAnalyticsService);
  private readonly _sheetDataService = inject(SheetDataService);
  private readonly _sheetCheckoutService = inject(SheetCheckoutService);
  private readonly _messageService = inject(MessageService);
  private readonly _dialog = inject(Dialog);
  private readonly _injector = inject(Injector);

  public async saveChangesAsync(): Promise<void> {
    const specSheet = this.specSheet();
    if (!specSheet) {
      return;
    }

    const updateRequest = specSheet.compileChanges();
    if (!updateRequest) {
      this._messageService.add({
        severity: 'info',
        summary: 'No Changes to Save',
      });
      return;
    }

    const spinnerDialogRef = this._dialog.open<
      unknown,
      SpinnerDialogData,
      SpinnerDialog
    >(SpinnerDialog, {
      data: {
        name: 'Saving Spec Sheet Changes',
        hint: 'Please wait while the spec sheet changes are being saved.',
      },
    });

    try {
      await this._sheetDataService.updateSheetAsync(updateRequest);

      this._messageService.add({
        severity: 'success',
        summary: 'Spec Sheet Changes Saved',
        detail:
          'The rows and tables you changed are checked out to you until you submit or undo them.',
      });
      this._activeSheetService.triggerReload();
      await this._sheetCheckoutService.notifyAsync('saved changes');
    } catch (err) {
      this._messageService.add({
        severity: 'error',
        summary: 'Error Saving Spec Sheet Changes',
        detail: SheetCheckoutService.reasonOf(err),
      });
    } finally {
      spinnerDialogRef.close();
    }
  }

  public async publishSpecSheetAsync(): Promise<void> {
    const publishDialogRef = this._dialog.open<
      PublishSheetRequest,
      unknown,
      PublishSpecSheetDialog
    >(PublishSpecSheetDialog, {
      injector: this._injector,
      minWidth: '50%',
    });
    const result = await firstValueFrom(publishDialogRef.closed);
    if (!result) {
      return;
    }

    const spinnerDialogRef = this._dialog.open<
      unknown,
      SpinnerDialogData,
      SpinnerDialog
    >(SpinnerDialog, {
      data: {
        name: 'Submitting Your Changes',
        hint: 'Please wait while your changes are submitted.',
      },
    });

    try {
      await this._sheetDataService.publishSheetAsync(result);

      const needsApproval = this.specSheet().requiresApprovalWorkflow;
      this._messageService.add({
        severity: 'success',
        summary: needsApproval
          ? 'Your Changes Were Submitted for Approval'
          : 'Your Changes Were Published',
      });
      this._sheetsService.reload();
      this._activeSheetService.triggerReload();
      await this._sheetCheckoutService.notifyAsync(
        needsApproval ? 'submitted changes for approval' : 'published changes',
      );
    } catch (err) {
      this._messageService.add({
        severity: 'error',
        summary: 'Error Submitting Your Changes',
        detail: SheetCheckoutService.reasonOf(err),
      });
    } finally {
      spinnerDialogRef.close();
    }
  }

  public async undoMineAsync(): Promise<void> {
    const count = this._activeSheetService.checkoutSummary().mine;
    const dialogRef = this._dialog.open<
      boolean,
      ConfirmationDialogData,
      ConfirmationDialog
    >(ConfirmationDialog, {
      data: {
        title: 'Undo My Changes',
        message: `Throw away your changes to the ${count} row(s) and table(s) you have checked out? They go back to the latest version and are free for anyone to edit. This can't be undone.`,
        confirmText: 'Undo',
        confirmSeverity: 'danger',
      },
    });

    const result = await firstValueFrom(dialogRef.closed);
    if (!result) {
      return;
    }

    await this._sheetCheckoutService.undoMineAsync();
  }

  public async addTable(tableType?: TableTemplate) {
    const specSheetType = this._sheetsService.selectedType();
    const user = this._userService.user();
    if (!specSheetType || !user) {
      return;
    }

    if (!tableType) {
      tableType = specSheetType.tableTemplates[0];
    }

    const groups: SheetTableGroup[] = [];
    const widgets: SheetWidget[] = [];
    if (tableType.requiresUserConfiguration) {
      const dialogRef = this._dialog.open<
        TableTemplate,
        TableTemplate,
        ConfigureTablePropertiesDialog
      >(ConfigureTablePropertiesDialog, {
        data: tableType,
        injector: this._injector,
      });
      const result = await firstValueFrom(dialogRef.closed);
      if (!result) {
        return;
      }

      tableType = { ...result };
    } else {
      const defaultGroups = tableType!.tableGroupTemplates.filter(
        (rt) => rt.isDefault,
      );
      if (!defaultGroups) {
        return;
      }

      let linkedGroupReferenceId: string | undefined = undefined;
      for (let i = 0; i < defaultGroups.length; i++) {
        const defaultGroup = defaultGroups[i];

        const rows: SheetTableRow[] = [];
        for (const row of defaultGroup.rowTemplates) {
          const cells: SheetTableCell[] = [];
          for (const cell of row.cellTemplates) {
            cells.push({
              id: nextTempId(),
              sheetTableRowId: 0,
              tableCellTemplateId: cell.id,
              cellConfigurationId: cell.cellConfigurationId,
              tableCellTypeId: cell.tableCellTypeId,
              referenceId: crypto.randomUUID(),
              columnNumber: cell.columnNumber,
              rowSpan: cell.rowSpan,
              colSpan: cell.colSpan,
              createdDateTime: DateTime.now().toISO(),
              cellConfiguration: cell.cellConfiguration,
              value: CellValueHelpers.buildNewCellValue(cell),
              tableCellType: cell.tableCellType,
            });
          }

          rows.push({
            id: nextTempId(),
            sheetTableGroupId: 0,
            revision: SheetItemHelpers.newRevision(user.id),
            tableRowTemplateId: row.id,
            referenceId: crypto.randomUUID(),
            displayOrder: row.rowNumber,
            cells,
            createdDateTime: DateTime.now().toISO(),
          });
        }

        const widgets: SheetWidget[] = [];
        for (const tableGroupTemplateWidget of defaultGroup.tableGroupTemplateWidgets) {
          widgets.push({
            id: nextTempId(),
            sheetTableGroupId: 0,
            tableGroupTemplateWidgetId: tableGroupTemplateWidget.id,
            value: undefined,
          });
        }

        const groupReferenceId = crypto.randomUUID();
        groups.push({
          id: nextTempId(),
          sheetTableId: 0,
          revision: SheetItemHelpers.newRevision(user.id),
          tableGroupTemplateId: defaultGroup.id,
          displayOrder: 1,
          referenceId: groupReferenceId,
          createdDateTime: DateTime.now().toISO(),
          rows,
          widgets,
          linkedGroupReferenceId: linkedGroupReferenceId,
        });

        linkedGroupReferenceId = groupReferenceId;
      }

      for (const tableTemplateWidget of tableType.tableTemplateWidgets) {
        widgets.push({
          id: nextTempId(),
          sheetTableId: 0,
          tableTemplateWidgetId: tableTemplateWidget.id,
          value: undefined,
        });
      }

      if (groups.length === 0) {
        return;
      }
    }

    const table: SheetTable = {
      id: nextTempId(),
      tableTemplateId: tableType.id,
      groups,
      widgets,
      title: '',
      displayOrder: this._activeSheetService.specSheet().tables().length + 1,
      referenceId: crypto.randomUUID(),
      sheetId: this._activeSheetService.specSheet().id,
      revision: SheetItemHelpers.newRevision(user.id),
      createdDateTime: DateTime.now().toISO(),
      configurationOverride: tableType.defaultConfiguration,
      notes: [],
    };

    this._activeSheetService.specSheet().addTable(table);
  }

  public get specSheet(): Signal<PUSpecSheet> {
    return this._activeSheetService.specSheet;
  }

  public get canEdit(): Signal<boolean> {
    return this._activeSheetService.canEdit;
  }

  public get isValid(): Signal<boolean> {
    return this._activeSheetService.isValid;
  }

  public get hasChanges(): Signal<boolean> {
    return this._activeSheetService.hasChanges;
  }

  public get canPublish(): Signal<boolean> {
    return this._activeSheetService.canPublish;
  }

  public get isAnalyticsEnabled(): Signal<boolean> {
    return this._puCardAnalyticsService.isEnabled;
  }
}
