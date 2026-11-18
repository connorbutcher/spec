import { Dialog } from '@angular/cdk/dialog';
import { inject, Injectable, Injector } from '@angular/core';
import { DateTime } from 'luxon';
import { firstValueFrom } from 'rxjs';
import { CellValueHelpers } from '../../../core/helpers/cell-value-helpers';
import { nextTempId } from '../../../core/helpers/temp-id-counter';
import { User } from '../../../core/models/domain/access/user';
import { TableCellTemplate } from '../../../core/models/domain/template/table-cell-template';
import { TableGroupTemplate } from '../../../core/models/domain/template/table-group-template';
import { TableRowTemplate } from '../../../core/models/domain/template/table-row-template';
import { UserService } from '../../../core/services/user.service';
import { SheetsService } from '../../sheets/services/sheets.service';
import { ConfigureCellPropertiesDialog } from '../dialogs/configure-cell-properties-dialog/configure-cell-properties-dialog';
import { PUSpecSheetTableGroup } from '../models/pu-spec-sheet-table-group';
import { PUSpecSheetTableBase } from '../models/tables/pu-spec-sheet-table-base';
import { ActiveSheetService } from './active-sheet.service';
import { SheetCellValue } from '../../../core/models/domain/sheets/sheet-cell-value';
import { SheetTableCell } from '../../../core/models/domain/sheets/sheet-table-cell';
import { SheetTableGroup } from '../../../core/models/domain/sheets/sheet-table-group';
import { SheetTableRow } from '../../../core/models/domain/sheets/sheet-table-row';
import { SheetWidget } from '../../../core/models/domain/sheets/sheet-widget';
import { SheetItemHelpers } from '../../../core/helpers/sheet-item-helpers';

@Injectable()
export class TableGroupBuilderService {
  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _sheetsService = inject(SheetsService);
  private readonly _userService = inject(UserService);
  private readonly _dialog = inject(Dialog);
  private readonly _injector = inject(Injector);

  public async addOptionAsync(
    tableGroupTemplate: TableGroupTemplate,
    table: PUSpecSheetTableBase,
    parentGroup?: PUSpecSheetTableGroup,
    newRowNumber?: number,
  ): Promise<void> {
    if (!this._activeSheetService.canEdit()) {
      return;
    }

    const user = this._userService.user();
    if (!user || !table) {
      return;
    }

    const hasCellsWithConfiguration = tableGroupTemplate.rowTemplates.some(
      (rt) =>
        rt.cellTemplates.some(
          (ct) => ct.tableCellType!.requiresUserConfiguration,
        ),
    );
    if (hasCellsWithConfiguration) {
      const configuredTableGroupTemplate =
        await this.configureCellsAsync(tableGroupTemplate);
      if (!configuredTableGroupTemplate) {
        return;
      }

      tableGroupTemplate = configuredTableGroupTemplate;
    }

    const hasGroupedCells = tableGroupTemplate.rowTemplates.some((rt) =>
      rt.cellTemplates.some((c) => c.groupKey),
    );
    if (hasGroupedCells) {
      this.addGroupedRows(
        tableGroupTemplate,
        user,
        table,
        parentGroup,
        newRowNumber,
      );
    } else {
      if (
        tableGroupTemplate.copyFromPrevious &&
        parentGroup
          ?.childGroups()
          .some((g) => g.tableGroupTemplate()?.id === tableGroupTemplate.id)
      ) {
        // const confirmationRef = this._dialog.open<
        //   boolean,
        //   ConfirmationDialogData,
        //   ConfirmationDialog
        // >(ConfirmationDialog, {
        //   data: {
        //     title: 'Copy Values from Previous',
        //     message:
        //       'Are you sure you want to copy values from the previous group?',
        //     confirmText: 'Yes',
        //     cancelText: 'No',
        //   },
        // });

        //const result = await firstValueFrom(confirmationRef.closed);
        // if (result) {
        this.addGroup(
          tableGroupTemplate,
          user,
          table,
          parentGroup,
          true,
          true,
          newRowNumber,
        );
        // } else {
        //   this.addGroup(
        //     tableGroupTemplate,
        //     user,
        //     table,
        //     parentGroup,
        //     true,
        //     false,
        //     newRowNumber,
        //   );
        // }

        return;
      }

      this.addGroup(
        tableGroupTemplate,
        user,
        table,
        parentGroup,
        false,
        false,
        newRowNumber,
      );
    }

    table.triggerRebuild();
  }

  private async configureCellsAsync(
    tableGroupTemplate: TableGroupTemplate,
  ): Promise<TableGroupTemplate | undefined> {
    const cellsToConfigure: TableCellTemplate[] = [];
    for (const rowTemplate of tableGroupTemplate.rowTemplates) {
      for (const cellTemplate of rowTemplate.cellTemplates) {
        if (cellTemplate.tableCellType!.requiresUserConfiguration) {
          cellsToConfigure.push(cellTemplate);
        }
      }
    }

    const dialogRef = this._dialog.open<
      TableCellTemplate[] | undefined,
      TableCellTemplate[],
      ConfigureCellPropertiesDialog
    >(ConfigureCellPropertiesDialog, {
      data: cellsToConfigure,
      injector: this._injector,
    });
    const result = await firstValueFrom(dialogRef.closed);
    if (!result) {
      return undefined;
    }

    const rowTemplates: TableRowTemplate[] = [];
    for (const rowTemplate of tableGroupTemplate.rowTemplates) {
      const cellTemplates: TableCellTemplate[] = [];
      for (const cellTemplate of rowTemplate.cellTemplates) {
        const modifiedCell = result.find((r) => r.id === cellTemplate.id);
        cellTemplates.push(modifiedCell ?? cellTemplate);
      }

      rowTemplates.push({
        ...rowTemplate,
        cellTemplates,
      });
    }

    return { ...tableGroupTemplate, rowTemplates };
  }

  private addGroup(
    tableGroupTemplate: TableGroupTemplate,
    user: User,
    table: PUSpecSheetTableBase,
    parentGroup?: PUSpecSheetTableGroup,
    copyFromPrevious = false,
    copyValuesFromPrevious = false,
    newRowNumber?: number,
  ): void {
    const rows: SheetTableRow[] = [];

    let previousGroup: PUSpecSheetTableGroup | undefined = undefined;
    if (copyFromPrevious && parentGroup) {
      previousGroup = parentGroup.childGroups()[0];
      for (const row of previousGroup.rows()) {
        let rowNumber = row.rowNumber();
        if (newRowNumber && rowNumber > 1) {
          rowNumber = newRowNumber + (rowNumber - 1);
        }

        const cells: SheetTableCell[] = [];
        for (let c = 0; c < row.cells().length; c++) {
          const cell = row.cells()[c];
          const tableCell = cell.toTableCell(
            copyValuesFromPrevious ? 'none' : 'all',
          );

          const cellValue = {
            ...tableCell.value,
            sheetTableCellId: 0,
          } as SheetCellValue;

          cells.push({
            id: nextTempId(),
            sheetTableRowId: 0,
            tableCellTemplateId: tableCell.tableCellTemplateId,
            tableCellTypeId: tableCell.tableCellTypeId,
            cellConfigurationId: tableCell.cellConfigurationId,
            columnNumber: tableCell.columnNumber,
            rowSpan: tableCell.rowSpan,
            colSpan: tableCell.colSpan,
            cellConfiguration: tableCell.cellConfiguration,
            createdDateTime: DateTime.now().toISO(),
            referenceId: crypto.randomUUID(),
            tableCellType: tableCell.tableCellType,
            value: cellValue,
            configurationOverride: tableCell.tableCellType!
              .requiresUserConfiguration
              ? (tableCell.configurationOverride ??
                tableCell.cellConfiguration?.properties)
              : undefined,
          });
        }

        rows.push({
          id: nextTempId(),
          sheetTableGroupId: 0,
          revision: SheetItemHelpers.newRevision(user.id),
          tableRowTemplateId: row.tableRowTemplate()!.id,
          referenceId: crypto.randomUUID(),
          createdDateTime: DateTime.now().toISO(),
          displayOrder: rowNumber,
          cells: cells,
        });
      }
    } else {
      for (const rowTemplate of tableGroupTemplate.rowTemplates) {
        let rowNumber = rowTemplate.rowNumber;
        if (newRowNumber && rowNumber > 1) {
          rowNumber = newRowNumber + (rowNumber - 1);
        }

        const cells: SheetTableCell[] = [];
        for (const cellTemplate of rowTemplate.cellTemplates) {
          cells.push({
            id: nextTempId(),
            sheetTableRowId: 0,
            tableCellTemplateId: cellTemplate.id,
            tableCellTypeId: cellTemplate.tableCellTypeId,
            cellConfigurationId: cellTemplate.cellConfigurationId,
            columnNumber: cellTemplate.columnNumber,
            rowSpan: cellTemplate.rowSpan,
            colSpan: cellTemplate.colSpan,
            cellConfiguration: cellTemplate.cellConfiguration,
            createdDateTime: DateTime.now().toISO(),
            referenceId: crypto.randomUUID(),
            tableCellType: cellTemplate.tableCellType,
            value: CellValueHelpers.buildNewCellValue(cellTemplate),
            configurationOverride: cellTemplate.tableCellType!
              .requiresUserConfiguration
              ? cellTemplate.cellConfiguration?.properties
              : undefined,
          });
        }

        rows.push({
          id: nextTempId(),
          sheetTableGroupId: 0,
          revision: SheetItemHelpers.newRevision(user.id),
          tableRowTemplateId: rowTemplate.id,
          referenceId: crypto.randomUUID(),
          createdDateTime: DateTime.now().toISO(),
          displayOrder: rowNumber,
          cells: cells,
        });
      }
    }

    const widgets: SheetWidget[] = [];
    for (const templateWidget of tableGroupTemplate.tableGroupTemplateWidgets) {
      widgets.push({
        id: nextTempId(),
        sheetTableGroupId: 0,
        tableGroupTemplateWidgetId: templateWidget.id,
        value: undefined,
      });
    }

    const group: SheetTableGroup = {
      id: nextTempId(),
      sheetTableId: table.id,
      tableGroupTemplateId: tableGroupTemplate.id,
      referenceId: crypto.randomUUID(),
      revision: SheetItemHelpers.newRevision(user.id),
      createdDateTime: DateTime.now().toISO(),
      rows,
      widgets: widgets,
      displayOrder:
        newRowNumber ??
        (parentGroup
          ? parentGroup.childGroups().length + 1
          : table.groups().length + 1),
      linkedGroupReferenceId: parentGroup?.referenceId(),
    };

    let rowInsertionIndex: number | undefined = undefined;
    if (this._sheetsService.selectedType()!.tableOrientation === 'horizontal') {
      rowInsertionIndex = 0;
    } else {
      rowInsertionIndex = newRowNumber ? newRowNumber - 1 : undefined;
    }

    if (parentGroup) {
      parentGroup.addGroup(group, rowInsertionIndex);
    } else {
      table.addGroup(group, rowInsertionIndex);
    }
  }

  private addGroupedRows(
    tableGroupTemplate: TableGroupTemplate,
    user: User,
    table: PUSpecSheetTableBase,
    parentGroup?: PUSpecSheetTableGroup,
    newRowNumber?: number,
  ): void {
    const groupedRowTemplates: Map<string, TableRowTemplate[]> = new Map();
    for (const rowTemplate of tableGroupTemplate.rowTemplates) {
      for (const cellTemplate of rowTemplate.cellTemplates) {
        if (!cellTemplate.groupKey) {
          continue;
        }

        if (!groupedRowTemplates.has(cellTemplate.groupKey)) {
          groupedRowTemplates.set(cellTemplate.groupKey, [
            { ...rowTemplate, cellTemplates: [cellTemplate] },
          ]);
        } else {
          const existing = groupedRowTemplates.get(cellTemplate.groupKey);
          const existingRow = existing!.find(
            (rt) => rt.rowNumber === rowTemplate.rowNumber,
          );
          if (existingRow) {
            existingRow.cellTemplates.push(cellTemplate);
          } else {
            existing!.push({
              ...rowTemplate,
              cellTemplates: [cellTemplate],
            });
          }
        }
      }
    }

    const parentGroupKey = parentGroup?.tableGroupTemplate()?.groupKey;
    if (parentGroupKey && groupedRowTemplates.has(parentGroupKey)) {
      const rowTemplates = groupedRowTemplates.get(parentGroupKey)!;

      const rows: SheetTableRow[] = [];
      for (const rowTemplate of rowTemplates) {
        const cells: SheetTableCell[] = [];
        for (const cellTemplate of rowTemplate.cellTemplates) {
          cells.push({
            id: nextTempId(),
            sheetTableRowId: 0,
            tableCellTemplateId: cellTemplate.id,
            tableCellTypeId: cellTemplate.tableCellTypeId,
            cellConfigurationId: cellTemplate.cellConfigurationId,
            columnNumber: cellTemplate.columnNumber,
            rowSpan: cellTemplate.rowSpan,
            colSpan: cellTemplate.colSpan,
            cellConfiguration: cellTemplate.cellConfiguration,
            createdDateTime: DateTime.now().toISO(),
            referenceId: crypto.randomUUID(),
            tableCellType: cellTemplate.tableCellType,
            value: CellValueHelpers.buildNewCellValue(cellTemplate),
            configurationOverride: cellTemplate.tableCellType!
              .requiresUserConfiguration
              ? cellTemplate.cellConfiguration?.properties
              : undefined,
          });
        }

        rows.push({
          id: nextTempId(),
          sheetTableGroupId: parentGroup!.id,
          revision: SheetItemHelpers.newRevision(user.id),
          tableRowTemplateId: rowTemplate.id,
          referenceId: crypto.randomUUID(),
          createdDateTime: DateTime.now().toISO(),
          displayOrder: parentGroup.rows().length + rowTemplate.rowNumber,
          cells: cells,
        });
      }

      for (const row of rows) {
        parentGroup.addRow(row, newRowNumber ? newRowNumber - 1 : undefined);
      }
    }

    for (const [groupKey, rowTemplates] of groupedRowTemplates) {
      if (groupKey === parentGroupKey) {
        continue;
      }

      const groups = parentGroup
        ?.childGroups()
        .filter((g) => g.tableGroupTemplate()?.groupKey === groupKey);
      if (!groups || groups.length === 0) {
        continue;
      }

      const rowsToAdd = new Map<PUSpecSheetTableGroup, SheetTableRow[]>();
      for (const group of groups) {
        const rows: SheetTableRow[] = [];
        for (const rowTemplate of rowTemplates) {
          const cells: SheetTableCell[] = [];
          for (const cellTemplate of rowTemplate.cellTemplates) {
            cells.push({
              id: nextTempId(),
              sheetTableRowId: 0,
              tableCellTemplateId: cellTemplate.id,
              tableCellTypeId: cellTemplate.tableCellTypeId,
              cellConfigurationId: cellTemplate.cellConfigurationId,
              columnNumber: cellTemplate.columnNumber,
              rowSpan: cellTemplate.rowSpan,
              colSpan: cellTemplate.colSpan,
              cellConfiguration: cellTemplate.cellConfiguration,
              createdDateTime: DateTime.now().toISO(),
              referenceId: crypto.randomUUID(),
              tableCellType: cellTemplate.tableCellType,
              value: CellValueHelpers.buildNewCellValue(cellTemplate),
              configurationOverride: cellTemplate.tableCellType!
                .requiresUserConfiguration
                ? cellTemplate.cellConfiguration?.properties
                : undefined,
            });
          }

          rows.push({
            id: nextTempId(),
            sheetTableGroupId: group.id,
            revision: SheetItemHelpers.newRevision(user.id),
            tableRowTemplateId: rowTemplate.id,
            referenceId: crypto.randomUUID(),
            createdDateTime: DateTime.now().toISO(),
            displayOrder: groups[0].rows().length + rowTemplate.rowNumber,
            cells: cells,
          });
        }

        rowsToAdd.set(group, rows);
      }

      for (const [group, rows] of rowsToAdd) {
        for (const row of rows) {
          group.addRow(row, newRowNumber ? newRowNumber - 1 : undefined);
        }
      }
    }
  }
}
