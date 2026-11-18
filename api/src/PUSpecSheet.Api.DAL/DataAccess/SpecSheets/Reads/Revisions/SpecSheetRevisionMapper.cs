using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database.Schema.Access;
using PUSpecSheet.Api.Database.Schema.Approval;
using PUSpecSheet.Api.Database.Schema.References;
using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Api.Database.Schema.SpecSheets;
using PUSpecSheet.Api.Database.Schema.Values;
using PUSpecSheet.Api.Database.Schema.Workflow;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads.Revisions;

/// <summary>
/// Maps what the revisions show of a sheet to the records a copy of the sheet is read into, so that what is read from
/// either is mapped to the domain the same way. Tables, groups, rows and cells have the ids of their spec items;
/// widgets, notes and workflow step actors, which the revisions keep inside their items, are numbered from 1 within
/// the sheet. An item's creator is who wrote the revision shown.
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

    public static List<DbSpecSheetTable> MapTables(DbSheet sheet, int specSheetId, RevisionItemNumbering ids, Dictionary<int, DbUser> users)
    {
        return [.. sheet.Tables
            .Where(table => table.Revisions.IsShown())
            .OrderBy(table => table.Id)
            .Select(table => MapTable(table, specSheetId, ids, users))
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

    private static DbSpecSheetTable MapTable(DbSheetTable table, int specSheetId, RevisionItemNumbering ids, Dictionary<int, DbUser> users)
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
            Groups = [.. table.Groups.Where(group => group.Revisions.IsShown()).OrderBy(group => group.Id).Select(group => MapGroup(group, groupIdsByReference, ids))],
            Widgets = [.. (revision.WidgetValues ?? []).Select(widget => MapTableWidget(widget, table.Id, ids))],
            Notes = [.. (revision.Notes ?? []).Select(note => MapNote(note, table.Id, ids, users))]
        };
    }

    private static DbSpecSheetTableWidget MapTableWidget(SheetWidgetValue widget, int tableId, RevisionItemNumbering ids)
    {
        var widgetId = ids.Next("table widget");
        return new DbSpecSheetTableWidget
        {
            Id = widgetId,
            SpecSheetTableId = tableId,
            TableTemplateWidgetId = widget.TemplateWidgetId,
            Value = new DbTableWidgetValue { Id = widgetId, SpecSheetTableWidgetId = widgetId, Value = widget.Value }
        };
    }

    private static DbSpecSheetTableNote MapNote(SheetTableNote note, int tableId, RevisionItemNumbering ids, Dictionary<int, DbUser> users)
    {
        return new DbSpecSheetTableNote
        {
            Id = ids.Next("note"),
            SpecSheetTableId = tableId,
            Note = note.Note,
            SourceSystem = note.SourceSystem,
            IsObsolete = note.IsObsolete,
            CreatedByUserId = note.CreatedByUserId,
            CreatedDateTime = note.CreatedDateTime,
            CreatedByUser = users.GetValueOrDefault(note.CreatedByUserId)
        };
    }

    private static DbSpecSheetTableGroup MapGroup(DbSheetTableGroup group, Dictionary<Guid, int> groupIdsByReference, RevisionItemNumbering ids)
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
            Widgets = [.. (revision.WidgetValues ?? []).Select(widget => MapGroupWidget(widget, group.Id, ids))],
            CreatedByUserId = revision.CreatedByUserId,
            CreatedDateTime = group.CreatedDateTime,
            LastModifiedByUserId = LastModifiedByUserId(revision),
            LastModifiedDateTime = LastModifiedDateTime(revision)
        };
    }

    private static DbSpecSheetTableGroupWidget MapGroupWidget(SheetWidgetValue widget, int groupId, RevisionItemNumbering ids)
    {
        var widgetId = ids.Next("group widget");
        return new DbSpecSheetTableGroupWidget
        {
            Id = widgetId,
            SpecSheetTableGroupId = groupId,
            TableGroupTemplateWidgetId = widget.TemplateWidgetId,
            Value = new DbTableGroupWidgetValue { Id = widgetId, SpecSheetTableGroupWidgetId = widgetId, Value = widget.Value }
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
