import { User } from '../models/domain/access/user';
import { PropertyValue } from '../models/domain/spec-sheet/properties/values/property-value';
import { SpecSheetPropertyValue } from '../models/domain/spec-sheet/properties/spec-sheet-property-value';
import { SpecSheetPropertyValueSummary } from '../models/domain/spec-sheet/properties/spec-sheet-property-value-summary';
import { SpecSheet } from '../models/domain/spec-sheet/spec-sheet';
import { SpecSheetSummary } from '../models/domain/spec-sheet/spec-sheet-summary';
import { SpecSheetTable } from '../models/domain/spec-sheet/spec-sheet-table';
import { SpecSheetTableCell } from '../models/domain/spec-sheet/spec-sheet-table-cell';
import { SpecSheetTableGroup } from '../models/domain/spec-sheet/spec-sheet-table-group';
import { SpecSheetTableNote } from '../models/domain/spec-sheet/spec-sheet-table-note';
import { SpecSheetTableRow } from '../models/domain/spec-sheet/spec-sheet-table-row';
import {
  Sheet,
  SheetCellValue,
  SheetExternalReference,
  SheetProperty,
  SheetSubmission,
  SheetSummary,
  SheetTable,
  SheetTableCell,
  SheetTableGroup,
  SheetTableNote,
  SheetTableRow,
  SheetVersionSummary,
  SheetWidget,
} from '../models/domain/sheets/sheet';
import { CellValue } from '../models/domain/values/cell-value';
import { SpecSheetExternalReference } from '../models/domain/references/spec-sheet-external-reference';
import { WorkflowStep } from '../models/domain/workflow/workflow-step';
import { WorkflowState } from '../models/domain/enums/workflow-state';
import { SubmissionStatus } from '../types/sheets/submission-status.enum';

/**
 * Turns what the sheets endpoints return into the models the sheet screens work with. A spec sheet there is identified
 * by the id of the SpecSheets row it is known by (a published version's, or a draft's), and each item by its own id,
 * which the edits it makes are addressed to.
 */
export class SheetAdapter {
  public static toSpecSheet(
    sheet: Sheet,
    specSheetId: number,
    submissions: SheetSubmission[] = [],
  ): SpecSheet {
    const version = sheet.isDraft ? undefined : sheet.version;
    const owner = sheet.version?.publishedByUser ?? ({} as User);
    return {
      id: specSheetId,
      specSheetPhaseId: sheet.specSheetPhaseId,
      specSheetTypeId: sheet.specSheetTypeId,
      versionNumber: (sheet.version?.versionNumber ?? 0) + (sheet.isDraft ? 1 : 0),
      isPublished: !sheet.isDraft,
      createdByUserId:
        sheet.version?.sourceCreatedByUserId ?? sheet.version?.publishedByUserId ?? 0,
      createdDateTime: sheet.version?.sourceCreatedDateTime ?? sheet.createdDateTime,
      createdByUser: owner,
      hasBeenMigrated: sheet.version?.hasBeenMigrated ?? false,
      publishedByUserId: version?.publishedByUserId,
      publishedDateTime: version?.publishedDateTime,
      publishedByUser: version?.publishedByUser,
      changeLog: version?.changeLog,
      specSheetPhase: sheet.specSheetPhase!,
      specSheetType: sheet.specSheetType!,
      tables: sheet.tables.map((t) => SheetAdapter.toTable(t, specSheetId)),
      propertyValues: sheet.properties.map((p) =>
        SheetAdapter.toPropertyValue(p, specSheetId),
      ),
      workflowSteps: submissions.map((s) => SheetAdapter.toWorkflowStep(s, specSheetId)),
      externalReferences: sheet.externalReferences.map((r) =>
        SheetAdapter.toExternalReference(r, specSheetId),
      ),
    };
  }

  /** A published version of a sheet, as a row of a list. */
  public static versionToSummary(summary: SheetVersionSummary): SpecSheetSummary {
    const version = summary.version;
    return {
      id: version.sourceSpecSheetId ?? 0,
      specSheetPhaseId: summary.specSheetPhaseId,
      specSheetTypeId: summary.specSheetTypeId,
      isPublished: true,
      createdByUserId: version.sourceCreatedByUserId ?? version.publishedByUserId,
      createdDateTime: version.sourceCreatedDateTime ?? version.publishedDateTime,
      createdByUser: version.publishedByUser ?? ({} as User),
      hasBeenMigrated: version.hasBeenMigrated,
      publishedByUserId: version.publishedByUserId,
      publishedDateTime: version.publishedDateTime,
      publishedByUser: version.publishedByUser,
      changeLog: version.changeLog,
      specSheetPhase: summary.specSheetPhase!,
      specSheetType: summary.specSheetType!,
      propertyValues: summary.properties.map((p) => SheetAdapter.toPropertyValueSummary(p)),
    };
  }

  /** The unpublished draft of a sheet, as a row of a list; undefined when it has none. */
  public static draftToSummary(summary: SheetSummary): SpecSheetSummary | undefined {
    if (summary.draftSpecSheetId === undefined || summary.draftSpecSheetId === null) {
      return undefined;
    }

    const latest = summary.latestVersion;
    return {
      id: summary.draftSpecSheetId,
      specSheetPhaseId: summary.specSheetPhaseId,
      specSheetTypeId: summary.specSheetTypeId,
      isPublished: false,
      createdByUserId: latest?.publishedByUserId ?? 0,
      createdDateTime: summary.createdDateTime,
      createdByUser: latest?.publishedByUser ?? ({} as User),
      hasBeenMigrated: latest?.hasBeenMigrated ?? false,
      specSheetPhase: summary.specSheetPhase!,
      specSheetType: summary.specSheetType!,
      propertyValues: summary.properties.map((p) => SheetAdapter.toPropertyValueSummary(p)),
    };
  }

  /** The published versions of the sheets, then their drafts, as the rows of a list. */
  public static toSummaries(
    versions: SheetVersionSummary[],
    sheets: SheetSummary[],
  ): SpecSheetSummary[] {
    const drafts = sheets
      .map((s) => SheetAdapter.draftToSummary(s))
      .filter((s): s is SpecSheetSummary => s !== undefined);
    return [...versions.map((v) => SheetAdapter.versionToSummary(v)), ...drafts];
  }

  private static toTable(table: SheetTable, specSheetId: number): SpecSheetTable {
    return {
      id: table.id,
      specSheetId: specSheetId,
      tableTemplateId: table.tableTemplateId,
      title: table.title,
      displayOrder: table.displayOrder,
      referenceId: table.referenceId,
      isDeleted: false,
      groups: table.groups.map((g) => SheetAdapter.toGroup(g, table)),
      widgets: table.widgets.map((w) => ({
        id: w.id,
        specSheetTableId: table.id,
        tableTemplateWidgetId: w.tableTemplateWidgetId!,
        value: { id: w.id, specSheetTableWidgetId: w.id, value: w.value },
      })),
      notes: table.notes.map((n) => SheetAdapter.toNote(n)),
      createdByUserId: table.revision.createdByUserId,
      createdDateTime: table.createdDateTime,
      createdByUser: table.revision.createdByUser,
      configurationOverride: table.configurationOverride,
    };
  }

  private static toGroup(group: SheetTableGroup, table: SheetTable): SpecSheetTableGroup {
    const parent = group.linkedGroupReferenceId
      ? table.groups.find((g) => g.referenceId === group.linkedGroupReferenceId)
      : undefined;
    return {
      id: group.id,
      specSheetTableId: group.sheetTableId,
      linkedSpecSheetTableGroupId: parent?.id,
      tableGroupTemplateId: group.tableGroupTemplateId,
      order: group.displayOrder,
      referenceId: group.referenceId,
      isDeleted: false,
      rows: group.rows.map((r) => SheetAdapter.toRow(r)),
      widgets: group.widgets.map((w: SheetWidget) => ({
        id: w.id,
        specSheetTableGroupId: group.id,
        tableGroupTemplateWidgetId: w.tableGroupTemplateWidgetId!,
        value: { id: w.id, specSheetTableGroupWidgetId: w.id, value: w.value },
      })),
      createdByUserId: group.revision.createdByUserId,
      createdDateTime: group.createdDateTime,
      createdByUser: group.revision.createdByUser,
    };
  }

  private static toRow(row: SheetTableRow): SpecSheetTableRow {
    return {
      id: row.id,
      specSheetTableGroupId: row.sheetTableGroupId,
      tableRowTemplateId: row.tableRowTemplateId,
      rowNumber: row.displayOrder,
      referenceId: row.referenceId,
      isDeleted: false,
      cells: row.cells.map((c) => SheetAdapter.toCell(c, row)),
      createdByUserId: row.revision.createdByUserId,
      createdDateTime: row.createdDateTime,
      createdByUser: row.revision.createdByUser,
    };
  }

  private static toCell(cell: SheetTableCell, row: SheetTableRow): SpecSheetTableCell {
    return {
      id: cell.id,
      specSheetTableRowId: cell.sheetTableRowId,
      tableCellTypeId: cell.tableCellTypeId,
      cellConfigurationId: cell.cellConfigurationId,
      rowNumber: row.displayOrder,
      columnNumber: cell.columnNumber,
      rowSpan: cell.rowSpan,
      colSpan: cell.colSpan,
      referenceId: cell.referenceId,
      isDeleted: false,
      cellValue: SheetAdapter.toCellValue(cell.value, cell.id),
      createdByUserId: row.revision.createdByUserId,
      createdDateTime: cell.createdDateTime,
      configurationOverride: cell.configurationOverride,
      cellConfiguration: cell.cellConfiguration!,
      tableCellType: cell.tableCellType!,
    };
  }

  // A cell has one value, which edits address by the id of the cell.
  private static toCellValue(value: SheetCellValue, cellId: number): CellValue {
    const { sheetTableCellId, ...rest } = value;
    return { ...rest, id: cellId, specSheetTableCellId: sheetTableCellId } as CellValue;
  }

  private static toNote(note: SheetTableNote): SpecSheetTableNote {
    return {
      id: note.id,
      specSheetTableId: note.sheetTableId,
      note: note.note,
      sourceSystem: note.sourceSystem,
      isObsolete: note.isObsolete,
      createdByUserId: note.createdByUserId,
      createdDateTime: note.createdDateTime,
      createdByUser: note.createdByUser!,
    };
  }

  private static toPropertyValue(
    property: SheetProperty,
    specSheetId: number,
  ): SpecSheetPropertyValue {
    return {
      id: property.id,
      specSheetId: specSheetId,
      specSheetTypePropertyId: property.specSheetTypePropertyId,
      propertyValue: SheetAdapter.toPropertyValueOf(property),
      specSheetTypeProperty: property.specSheetTypeProperty!,
    };
  }

  private static toPropertyValueSummary(property: SheetProperty): SpecSheetPropertyValueSummary {
    const typeProperty = property.specSheetTypeProperty!;
    return {
      propertyValue: SheetAdapter.toPropertyValueOf(property)!,
      propertyKey: typeProperty.valueKey,
      propertyName: typeProperty.valueName,
      referenceId: typeProperty.referenceId,
      valueControlType: typeProperty.valueControlType,
    };
  }

  // The value is kept as text, and is the kind the property's type says it is.
  private static toPropertyValueOf(property: SheetProperty): PropertyValue | undefined {
    const type = property.specSheetTypeProperty?.valueType;
    if (!type) {
      return undefined;
    }

    const text = property.value;
    const empty = text === undefined || text === null || text.trim() === '';
    let value: string | number | boolean | undefined;
    switch (type) {
      case 'int':
        value = empty ? undefined : parseInt(text!, 10);
        break;
      case 'double':
        value = empty ? undefined : parseFloat(text!);
        break;
      case 'bool':
        value = empty ? undefined : text!.toLowerCase() === 'true';
        break;
      default:
        value = empty ? undefined : text!;
        break;
    }

    return { $type: type, value } as PropertyValue;
  }

  private static toExternalReference(
    reference: SheetExternalReference,
    specSheetId: number,
  ): SpecSheetExternalReference {
    return {
      id: reference.id,
      specSheetId: specSheetId,
      externalId: reference.externalId,
      referenceId: reference.referenceId,
      systemName: reference.systemName,
      ownerName: reference.ownerName,
      url: reference.url,
      type: reference.type,
      grouping: reference.grouping,
      systemIconUrl: reference.systemIconUrl,
      description: reference.description,
    };
  }

  // A submission is a step of the approval, and who may approve it are its actors, which are the users themselves.
  private static toWorkflowStep(submission: SheetSubmission, specSheetId: number): WorkflowStep {
    return {
      id: submission.id,
      specSheetId: specSheetId,
      startedDateTime: submission.submittedDateTime,
      state: SheetAdapter.toWorkflowState(submission.status),
      actors: submission.approvers.map((a) => ({
        id: a.userId,
        workflowStepId: submission.id,
        state: a.decision as WorkflowState,
        comment: a.reason,
        actionedDateTime: a.decidedDateTime,
        actor: a.user,
      })),
    };
  }

  private static toWorkflowState(status: number): WorkflowState {
    switch (status) {
      case SubmissionStatus.Pending:
        return WorkflowState.Pending;
      case SubmissionStatus.Approved:
        return WorkflowState.Approved;
      default:
        return WorkflowState.Rejected;
    }
  }
}
