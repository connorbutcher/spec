using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database.Schema.Access;
using PUSpecSheet.Api.Database.Schema.Approval;
using PUSpecSheet.Api.Database.Schema.Cell;
using PUSpecSheet.Api.Database.Schema.Legacy.References;
using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;
using PUSpecSheet.Api.Database.Schema.Legacy.Values;
using PUSpecSheet.Api.Database.Schema.Legacy.Workflow;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads.Revisions;

/// <summary>
/// Maps what the revisions show of a sheet to the records a copy of the sheet is read into, so that what is read from
/// either is mapped to the domain the same way. Tables, groups, rows, cells, widgets and notes have the ids of their
/// spec items; workflow step actors, which the revisions don't keep as items, are numbered from 1 within the sheet. An
/// item's creator is who wrote the revision shown, except a note's, which the note keeps.
/// </summary>
internal static class SpecSheetRevisionMapper
{
    private static int? LastModifiedByUserId(IDbSheetRevision revision)
    {
        return revision.RevisionNumber > 1 ? revision.CreatedByUserId : null;
    }

    private static DateTime? LastModifiedDateTime(IDbSheetRevision revision)
    {
        return revision.RevisionNumber > 1 ? revision.PublishedDateTime ?? revision.LastModifiedDateTime : null;
    }

    public static List<DbSpecSheetTable> MapTables(DbSheet sheet, int specSheetId, Dictionary<int, DbUser> users)
    {
        return [.. sheet.Tables
            .Where(table => table.Revisions.IsShown())
            .OrderBy(table => table.Id)
            .Select(table => MapTable(table, specSheetId, users))
            .OrderBy(table => table.DisplayOrder)];
    }

    public static List<DbSpecSheetPropertyValue> MapProperties(DbSheet sheet, int specSheetId)
    {
        return [.. sheet.Properties
            .Where(property => property.Revisions.IsShown())
            .Select(property => MapProperty(property, specSheetId))];
    }

    public static List<DbSpecSheetExternalReference> MapExternalReferences(DbSheet sheet, int specSheetId)
    {
        return [.. sheet.ExternalReferences
            .Where(reference => reference.Revisions.Count > 0)
            .Select(reference => MapExternalReference(reference, specSheetId))];
    }

    private static DbSpecSheetTable MapTable(DbSheetTable table, int specSheetId, Dictionary<int, DbUser> users)
    {
        var revision = table.Revisions.Shown();

        // A group is nested under the group it is linked to, which the revisions keep by ReferenceID.
        var groupIdsByReference = table.Groups
            .Where(group => group.Revisions.IsShown())
            .GroupBy(group => group.ReferenceId)
            .ToDictionary(group => group.Key, group => group.First().Id);

        return new DbSpecSheetTable
        {
            Id = table.Id,
            SpecSheetId = specSheetId,
            TableTemplateId = table.TableTemplateId,
            Title = revision.Title,
            DisplayOrder = revision.DisplayOrder,
            ReferenceId = table.ReferenceId,
            IsDeleted = false,
            CreatedByUserId = revision.CreatedByUserId,
            CreatedDateTime = table.CreatedDateTime,
            LastModifiedByUserId = LastModifiedByUserId(revision),
            LastModifiedDateTime = LastModifiedDateTime(revision),
            ConfigurationOverride = revision.ConfigurationOverride,
            Groups = [.. table.Groups.Where(group => group.Revisions.IsShown()).OrderBy(group => group.Id).Select(group => MapGroup(group, groupIdsByReference))],
            Widgets = [.. revision.WidgetValues.OrderBy(widget => widget.SheetTableWidgetId).Select(widget => MapTableWidget(widget, table.Id))],
            Notes = [.. revision.NoteValues.OrderBy(note => note.SheetTableNoteId).Select(note => MapNote(note, table.Id, users))]
        };
    }

    private static DbSpecSheetTableWidget MapTableWidget(DbSheetTableWidgetValue value, int tableId)
    {
        var widget = value.SheetTableWidget!;
        return new DbSpecSheetTableWidget
        {
            Id = widget.Id,
            SpecSheetTableId = tableId,
            TableTemplateWidgetId = widget.TableTemplateWidgetId,
            Value = new DbTableWidgetValue { Id = widget.Id, SpecSheetTableWidgetId = widget.Id, Value = value.Value }
        };
    }

    private static DbSpecSheetTableNote MapNote(DbSheetTableNoteValue value, int tableId, Dictionary<int, DbUser> users)
    {
        var note = value.SheetTableNote!;
        return new DbSpecSheetTableNote
        {
            Id = note.Id,
            SpecSheetTableId = tableId,
            Note = value.Note,
            SourceSystem = note.SourceSystem,
            IsObsolete = value.IsObsolete,
            CreatedByUserId = note.CreatedByUserId,
            CreatedDateTime = note.CreatedDateTime,
            CreatedByUser = users.GetValueOrDefault(note.CreatedByUserId)
        };
    }

    private static DbSpecSheetTableGroup MapGroup(DbSheetTableGroup group, Dictionary<Guid, int> groupIdsByReference)
    {
        var revision = group.Revisions.Shown();
        int? linkedGroupId = revision.LinkedSheetTableGroupReferenceId is { } linkedReference
            && groupIdsByReference.TryGetValue(linkedReference, out var linked)
                ? linked
                : null;

        return new DbSpecSheetTableGroup
        {
            Id = group.Id,
            SpecSheetTableId = group.SheetTableId,
            TableGroupTemplateId = group.TableGroupTemplateId,
            LinkedSpecSheetTableGroupId = linkedGroupId,
            Order = revision.DisplayOrder,
            ReferenceId = group.ReferenceId,
            IsDeleted = false,
            Rows = [.. group.Rows.Where(row => row.Revisions.IsShown()).Select(MapRow).OrderBy(row => row.RowNumber)],
            Widgets = [.. revision.WidgetValues.OrderBy(widget => widget.SheetTableGroupWidgetId).Select(widget => MapGroupWidget(widget, group.Id))],
            CreatedByUserId = revision.CreatedByUserId,
            CreatedDateTime = group.CreatedDateTime,
            LastModifiedByUserId = LastModifiedByUserId(revision),
            LastModifiedDateTime = LastModifiedDateTime(revision)
        };
    }

    private static DbSpecSheetTableGroupWidget MapGroupWidget(DbSheetTableGroupWidgetValue value, int groupId)
    {
        var widget = value.SheetTableGroupWidget!;
        return new DbSpecSheetTableGroupWidget
        {
            Id = widget.Id,
            SpecSheetTableGroupId = groupId,
            TableGroupTemplateWidgetId = widget.TableGroupTemplateWidgetId,
            Value = new DbTableGroupWidgetValue { Id = widget.Id, SpecSheetTableGroupWidgetId = widget.Id, Value = value.Value }
        };
    }

    private static DbSpecSheetTableRow MapRow(DbSheetTableRow row)
    {
        var revision = row.Revisions.Shown();
        var values = new RowRevisionValues(revision);

        var mapped = new DbSpecSheetTableRow
        {
            Id = row.Id,
            SpecSheetTableGroupId = row.SheetTableGroupId,
            TableRowTemplateId = row.TableRowTemplateId,
            RowNumber = revision.DisplayOrder,
            ReferenceId = row.ReferenceId,
            IsDeleted = false,
            CreatedByUserId = revision.CreatedByUserId,
            CreatedDateTime = row.CreatedDateTime,
            LastModifiedByUserId = LastModifiedByUserId(revision),
            LastModifiedDateTime = LastModifiedDateTime(revision)
        };

        foreach (var cell in row.Cells.OrderBy(cell => cell.TableCellTemplate!.ColumnNumber))
        {
            mapped.Cells.Add(MapCell(cell, mapped, values));
        }

        return mapped;
    }

    // A cell takes its place, type and configuration from its template cell, and who made and changed it from its row.
    private static DbSpecSheetTableCell MapCell(DbSheetTableCell cell, DbSpecSheetTableRow row, RowRevisionValues values)
    {
        var templateCell = cell.TableCellTemplate!;
        var mapped = new DbSpecSheetTableCell
        {
            Id = cell.Id,
            SpecSheetTableRowId = row.Id,
            TableCellTypeId = templateCell.TableCellTypeId,
            CellConfigurationId = templateCell.CellConfigurationId,
            RowNumber = row.RowNumber,
            ColumnNumber = templateCell.ColumnNumber,
            ColSpan = templateCell.ColSpan,
            RowSpan = templateCell.RowSpan,
            ReferenceId = cell.ReferenceId,
            IsDeleted = false,
            CreatedByUserId = row.CreatedByUserId,
            CreatedDateTime = cell.CreatedDateTime,
            LastModifiedByUserId = row.LastModifiedByUserId,
            LastModifiedDateTime = row.LastModifiedDateTime,
            ConfigurationOverride = values.ConfigurationOverrideOf(cell.Id),
            TableCellType = templateCell.TableCellType,
            CellConfiguration = templateCell.CellConfiguration
        };

        values.SetValue(mapped, templateCell.TableCellType!.ModelType);
        return mapped;
    }

    private static DbSpecSheetPropertyValue MapProperty(DbSheetProperty property, int specSheetId)
    {
        return new DbSpecSheetPropertyValue
        {
            Id = property.Id,
            SpecSheetId = specSheetId,
            SpecSheetTypePropertyId = property.SpecSheetTypePropertyId,
            PropertyValue = property.Revisions.Shown().PropertyValue,
            SpecSheetTypeProperty = property.SpecSheetTypeProperty
        };
    }

    private static DbSpecSheetExternalReference MapExternalReference(DbSheetExternalReference reference, int specSheetId)
    {
        var revision = reference.Revisions.Single();
        return new DbSpecSheetExternalReference
        {
            Id = reference.Id,
            SpecSheetId = specSheetId,
            ExternalId = revision.ExternalId,
            ReferenceId = reference.ReferenceId,
            SystemName = revision.SystemName,
            OwnerName = revision.OwnerName,
            Url = revision.Url,
            Type = revision.Type,
            Grouping = revision.Grouping,
            SystemIconUrl = revision.SystemIconUrl,
            Description = revision.Description
        };
    }

    // A submission as the workflow step it was migrated from. A withdrawn submission was a step left pending.
    public static DbWorkflowStep MapSubmission(DbSheetSubmission submission, int specSheetId, RevisionItemNumbering ids)
    {
        return new DbWorkflowStep
        {
            Id = submission.Id,
            SpecSheetId = specSheetId,
            StartedDateTime = submission.SubmittedDateTime,
            State = submission.Status switch
            {
                SubmissionStatus.Approved => WorkflowState.Approved,
                SubmissionStatus.Rejected => WorkflowState.Rejected,
                _ => WorkflowState.Pending
            },
            Actors = [.. submission.Approvers
                .OrderBy(approver => approver.UserId)
                .Select(approver => new DbWorkflowStepActor
                {
                    Id = ids.Next("actor"),
                    WorkflowStepId = submission.Id,
                    ActorUserId = approver.UserId,
                    ActorUser = approver.User,
                    State = approver.Decision switch
                    {
                        ApproverDecision.Approved => WorkflowState.Approved,
                        ApproverDecision.Rejected => WorkflowState.Rejected,
                        _ => WorkflowState.Pending
                    },
                    Comment = approver.Reason == DbSheetSubmissionApprover.NoReasonGiven ? null : approver.Reason,
                    ActionedDateTime = approver.DecidedDateTime
                })]
        };
    }
}
