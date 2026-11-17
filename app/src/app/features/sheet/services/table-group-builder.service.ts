import { Dialog } from '@angular/cdk/dialog';
import { inject, Injectable, Injector } from '@angular/core';
import { DateTime } from 'luxon';
import { firstValueFrom } from 'rxjs';
import { CellValueHelpers } from '../../../core/helpers/cell-value-helpers';
import { nextTempId } from '../../../core/helpers/temp-id-counter';
import { User } from '../../../core/models/domain/access/user';
import { SpecSheetTableCell } from '../../../core/models/domain/spec-sheet/spec-sheet-table-cell';
import { SpecSheetTableGroup } from '../../../core/models/domain/spec-sheet/spec-sheet-table-group';
import { SpecSheetTableGroupWidget } from '../../../core/models/domain/spec-sheet/spec-sheet-table-group-widget';
import { SpecSheetTableRow } from '../../../core/models/domain/spec-sheet/spec-sheet-table-row';
import { TableCellTemplate } from '../../../core/models/domain/template/table-cell-template';
import { TableGroupTemplate } from '../../../core/models/domain/template/table-group-template';
import { TableRowTemplate } from '../../../core/models/domain/template/table-row-template';
import { CellValue } from '../../../core/models/domain/values/cell-value';
import { UserService } from '../../../core/services/user.service';
import { SheetsService } from '../../sheets/services/sheets.service';
import { ConfigureCellPropertiesDialog } from '../dialogs/configure-cell-properties-dialog/configure-cell-properties-dialog';
import { PUSpecSheetTableGroup } from '../models/pu-spec-sheet-table-group';
import { PUSpecSheetTableBase } from '../models/tables/pu-spec-sheet-table-base';
import { ActiveSheetService } from './active-sheet.service';

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
    const rows: SpecSheetTableRow[] = [];

    let previousGroup: PUSpecSheetTableGroup | undefined = undefined;
    if (copyFromPrevious && parentGroup) {
      previousGroup = parentGroup.childGroups()[0];
      for (const row of previousGroup.rows()) {
        let rowNumber = row.rowNumber();
        if (newRowNumber && rowNumber > 1) {
          rowNumber = newRowNumber + (rowNumber - 1);
        }

        const cells: SpecSheetTableCell[] = [];
        for (let c = 0; c < row.cells().length; c++) {
          const cell = row.cells()[c];
          const tableCell = cell.toTableCell(
            copyValuesFromPrevious ? 'none' : 'all',
          );

          const cellValue = {
            ...tableCell.cellValue,
            id: nextTempId(),
            specSheetTableCellId: 0,
          } as CellValue;

          let cellRowNumber = tableCell.rowNumber;
          if (newRowNumber && cellRowNumber > 1) {
            cellRowNumber = newRowNumber + (cellRowNumber - 1);
          }

          cells.push({
            id: nextTempId(),
            specSheetTableRowId: 0,
            tableCellTypeId: tableCell.tableCellTypeId,
            cellConfigurationId: tableCell.cellConfigurationId,
            rowNumber: cellRowNumber,
            columnNumber: tableCell.columnNumber,
            rowSpan: tableCell.rowSpan,
            colSpan: tableCell.colSpan,
            cellConfiguration: tableCell.cellConfiguration,
            createdByUserId: user.id,
            createdDateTime: DateTime.now().toISO(),
            createdByUser: user,
            isDeleted: false,
            referenceId: crypto.randomUUID(),
            tableCellType: tableCell.tableCellType,
            cellValue: cellValue,
            configurationOverride: tableCell.tableCellType!
              .requiresUserConfiguration
              ? (tableCell.configurationOverride ??
                tableCell.cellConfiguration?.properties)
              : undefined,
          });
        }

        rows.push({
          id: nextTempId(),
          specSheetTableGroupId: 0,
          tableRowTemplateId: row.tableRowTemplate()!.id,
          referenceId: crypto.randomUUID(),
          createdByUserId: user.id,
          createdByUser: user,
          createdDateTime: DateTime.now().toISO(),
          isDeleted: false,
          rowNumber: rowNumber,
          cells: cells,
        });
      }
    } else {
      for (const rowTemplate of tableGroupTemplate.rowTemplates) {
        let rowNumber = rowTemplate.rowNumber;
        if (newRowNumber && rowNumber > 1) {
          rowNumber = newRowNumber + (rowNumber - 1);
        }

        const cells: SpecSheetTableCell[] = [];
        for (const cellTemplate of rowTemplate.cellTemplates) {
          cells.push({
            id: nextTempId(),
            specSheetTableRowId: 0,
            tableCellTypeId: cellTemplate.tableCellTypeId,
            cellConfigurationId: cellTemplate.cellConfigurationId,
            rowNumber: rowNumber,
            columnNumber: cellTemplate.columnNumber,
            rowSpan: cellTemplate.rowSpan,
            colSpan: cellTemplate.colSpan,
            cellConfiguration: cellTemplate.cellConfiguration,
            createdByUserId: user.id,
            createdDateTime: DateTime.now().toISO(),
            createdByUser: user,
            isDeleted: false,
            referenceId: crypto.randomUUID(),
            tableCellType: cellTemplate.tableCellType,
            cellValue: CellValueHelpers.buildNewCellValue(cellTemplate),
            configurationOverride: cellTemplate.tableCellType!
              .requiresUserConfiguration
              ? cellTemplate.cellConfiguration?.properties
              : undefined,
          });
        }

        rows.push({
          id: nextTempId(),
          specSheetTableGroupId: 0,
          tableRowTemplateId: rowTemplate.id,
          referenceId: crypto.randomUUID(),
          createdByUserId: user.id,
          createdByUser: user,
          createdDateTime: DateTime.now().toISO(),
          isDeleted: false,
          rowNumber: rowNumber,
          cells: cells,
        });
      }
    }

    const widgets: SpecSheetTableGroupWidget[] = [];
    for (const templateWidget of tableGroupTemplate.tableGroupTemplateWidgets) {
      widgets.push({
        id: nextTempId(),
        specSheetTableGroupId: 0,
        tableGroupTemplateWidgetId: templateWidget.id,
        value: {
          id: nextTempId(),
          specSheetTableGroupWidgetId: 0,
          value: undefined,
        },
      });
    }

    const group: SpecSheetTableGroup = {
      id: nextTempId(),
      specSheetTableId: table.id,
      tableGroupTemplateId: tableGroupTemplate.id,
      referenceId: crypto.randomUUID(),
      createdByUserId: user.id,
      createdByUser: user,
      createdDateTime: DateTime.now().toISO(),
      isDeleted: false,
      rows,
      widgets: widgets,
      order:
        newRowNumber ??
        (parentGroup
          ? parentGroup.childGroups().length + 1
          : table.groups().length + 1),
      linkedSpecSheetTableGroupId: parentGroup?.id,
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

      const rows: SpecSheetTableRow[] = [];
      for (const rowTemplate of rowTemplates) {
        const cells: SpecSheetTableCell[] = [];
        for (const cellTemplate of rowTemplate.cellTemplates) {
          cells.push({
            id: nextTempId(),
            specSheetTableRowId: 0,
            tableCellTypeId: cellTemplate.tableCellTypeId,
            cellConfigurationId: cellTemplate.cellConfigurationId,
            rowNumber: parentGroup.rows().length + rowTemplate.rowNumber,
            columnNumber: cellTemplate.columnNumber,
            rowSpan: cellTemplate.rowSpan,
            colSpan: cellTemplate.colSpan,
            cellConfiguration: cellTemplate.cellConfiguration,
            createdByUserId: user.id,
            createdDateTime: DateTime.now().toISO(),
            createdByUser: user,
            isDeleted: false,
            referenceId: crypto.randomUUID(),
            tableCellType: cellTemplate.tableCellType,
            cellValue: CellValueHelpers.buildNewCellValue(cellTemplate),
            configurationOverride: cellTemplate.tableCellType!
              .requiresUserConfiguration
              ? cellTemplate.cellConfiguration?.properties
              : undefined,
          });
        }

        rows.push({
          id: nextTempId(),
          specSheetTableGroupId: parentGroup!.id,
          tableRowTemplateId: rowTemplate.id,
          referenceId: crypto.randomUUID(),
          createdByUserId: user.id,
          createdByUser: user,
          createdDateTime: DateTime.now().toISO(),
          isDeleted: false,
          rowNumber: parentGroup.rows().length + rowTemplate.rowNumber,
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

      const rowsToAdd = new Map<PUSpecSheetTableGroup, SpecSheetTableRow[]>();
      for (const group of groups) {
        const rows: SpecSheetTableRow[] = [];
        for (const rowTemplate of rowTemplates) {
          const cells: SpecSheetTableCell[] = [];
          for (const cellTemplate of rowTemplate.cellTemplates) {
            cells.push({
              id: nextTempId(),
              specSheetTableRowId: 0,
              tableCellTypeId: cellTemplate.tableCellTypeId,
              cellConfigurationId: cellTemplate.cellConfigurationId,
              rowNumber: groups[0].rows().length + rowTemplate.rowNumber,
              columnNumber: cellTemplate.columnNumber,
              rowSpan: cellTemplate.rowSpan,
              colSpan: cellTemplate.colSpan,
              cellConfiguration: cellTemplate.cellConfiguration,
              createdByUserId: user.id,
              createdDateTime: DateTime.now().toISO(),
              createdByUser: user,
              isDeleted: false,
              referenceId: crypto.randomUUID(),
              tableCellType: cellTemplate.tableCellType,
              cellValue: CellValueHelpers.buildNewCellValue(cellTemplate),
              configurationOverride: cellTemplate.tableCellType!
                .requiresUserConfiguration
                ? cellTemplate.cellConfiguration?.properties
                : undefined,
            });
          }

          rows.push({
            id: nextTempId(),
            specSheetTableGroupId: group.id,
            tableRowTemplateId: rowTemplate.id,
            referenceId: crypto.randomUUID(),
            createdByUserId: user.id,
            createdByUser: user,
            createdDateTime: DateTime.now().toISO(),
            isDeleted: false,
            rowNumber: groups[0].rows().length + rowTemplate.rowNumber,
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
